using ALedgerBFFApi.Extension;
using ALedgerBFFApi.Model.Options;
using Amazon.S3;
using Amazon.S3.Model;
using HandlebarsDotNet;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;

namespace ALedgerBFFApi.Controllers
{
    [Authorize]
    [Route("v1/[controller]")]
    [ApiController]
    public class InvoiceController : Controller
    {
        private readonly OpenApiClient.Client client;
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IOptionsMonitor<ObjectStorage> objectStorageConfig;
        private readonly ILogger<InvoiceController> logger;
        public InvoiceController(ILogger<InvoiceController> logger, IOptionsMonitor<ObjectStorage> objectStorageConfig, IOptionsMonitor<BFF> bffConfig) : base()
        {
            this.logger = logger;
            this.objectStorageConfig = objectStorageConfig;
            client = new OpenApiClient.Client(bffConfig.CurrentValue.DataServer, httpClient);
        }

        [HttpPost("invoice")]
        public async Task<ActionResult<OpenApiClient.InvoiceDBBase>> NewInvoice([FromBody] Model.NewInvoice invoice)
        {
            httpClient.PassHeaders(Request);
            var dbInvoice = new OpenApiClient.Invoice
            {
                Currency = invoice.Currency,
                DateDelivery = invoice.DateDelivery,
                DateDue = invoice.DateDue,                
                DateIssue = invoice.DateIssue,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceNumberNum = invoice.InvoiceNumberNum,
                InvoiceType = invoice.InvoiceType,
                NoteAfterItems = invoice.NoteAfterItems,
                NoteBeforeItems = invoice.NoteBeforeItems,
                PaymentMethods = invoice.PaymentMethods?.Select(p => new OpenApiClient.PaymentMethod
                {
                   Account = p.Account,
                   Currency = p.Currency,
                   CurrencyId = p.CurrencyId,
                   GrossAmount = p.GrossAmount,
                   Network = p.Network
                }).ToArray() ?? null,
                PersonIdIssuer = invoice.PersonIdIssuer,
                PersonIdReceiver = invoice.PersonIdReceiver,
                Summary = invoice.Summary?.Select(s => new OpenApiClient.InvoiceSummaryInCurrency
                {
                    NetAmount = s.NetAmount,
                    Currency = s.Currency,
                    GrossAmount = s.GrossAmount,
                    Rate = s.Rate,
                    RateCurrencies = s.RateCurrencies,
                    RateNote = s.RateNote,
                    TaxAmount = s.TaxAmount
                }).ToArray() ?? null,
                Items = invoice.Items?.Select(item => new OpenApiClient.InvoiceItem
                {
                    Discount = item.Discount,
                    GrossAmount = item.GrossAmount,
                    ItemText = item.ItemText,
                    NetAmount = item.NetAmount,
                    Quantity = item.Quantity,
                    TaxPercent = item.TaxPercent,
                    Unit = item.Unit,
                    UnitPrice = item.UnitPrice
                }).ToArray() ?? null
            };           

            var result = await client.InvoicePostAsync(dbInvoice);

            return result;
        }

        [HttpPatch("invoice/{id}")]
        public async Task<ActionResult<OpenApiClient.Invoice>> PatchInvoice(string id, [FromBody] Model.NewInvoice invoice)
        {
            httpClient.PassHeaders(Request);
            var dbInvoice = await client.InvoiceGetByIdAsync(id);
            if (dbInvoice == null || dbInvoice.Data == null)
            {
                return NotFound();
            }
            else
            {
                var operations = await ConvertRecord2Patch(dbInvoice.Data, invoice);                
                var result = await client.InvoicePatchAsync(id, operations);
                return result.Data;
            }
        }

        [HttpDelete("invoice/{id}")]
        public async Task<ActionResult<OpenApiClient.Invoice>> DeleteInvoice(string id)
        {
            httpClient.PassHeaders(Request);
            var dbInvoice = await client.InvoiceGetByIdAsync(id);
            if (dbInvoice == null || dbInvoice.Data == null)
            {
                return NotFound();
            }
            else
            {
                var result = await client.InvoiceDeleteAsync(id);
                return result.Data;
            }
        }

        [HttpGet("invoice/{id}")]
        public async Task<ActionResult<OpenApiClient.Invoice>> GetInvoice(string id)
        {
            httpClient.PassHeaders(Request);
            var dbInvoice = await client.InvoiceGetByIdAsync(id);
            if (dbInvoice == null || dbInvoice.Data == null)
            {
                return NotFound();
            }
            else
            {
                return dbInvoice.Data;
            }
        }

        [HttpGet("invoice")]
        public async Task<ActionResult<IEnumerable<OpenApiClient.InvoiceDBBase>>> GetInvoices([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string? query = null, [FromQuery] string? sort = null)
        {
            httpClient.PassHeaders(Request);
            var addressList = await client.InvoiceGetAsync(offset, limit, query, sort);
            return addressList.Results.ToList();
        }

        private async Task<List<OpenApiClient.InvoiceOperation>> ConvertRecord2Patch(OpenApiClient.Invoice original, Model.NewInvoice invoice)
        {
            try
            {
                var retOperations = new List<OpenApiClient.InvoiceOperation>(); 
                foreach (PropertyInfo propertyInfoOriginal in original.GetType().GetProperties())
                {
                    var origValue = JsonConvert.SerializeObject(propertyInfoOriginal.GetValue(original));
                    var propertyInfo = invoice.GetType().GetProperty(propertyInfoOriginal.Name);
                    if (propertyInfo == null) 
                    {
                        logger.LogError("Property name does not exist");
                        continue;
                    }
                    var updatedValue = JsonConvert.SerializeObject(propertyInfo?.GetValue(invoice));
                    if (
                        origValue == null && updatedValue != null ||
                        updatedValue == null && origValue != null ||
                        (origValue != null && !origValue.Equals(updatedValue)))
                    {
                        var op = new OpenApiClient.InvoiceOperation()
                        {
                            Op = "replace",
                            //OperationType = 
                            Path = propertyInfo.Name,
                            Value = propertyInfo.GetValue(invoice)
                        };
                        retOperations.Add(op);
                    }
                }

                return retOperations;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}