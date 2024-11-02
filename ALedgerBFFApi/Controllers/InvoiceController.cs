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
using RestSharp.Serializers;
using System.IO;
using System.Reflection;
using System.Text;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ALedgerBFFApi.Controllers
{
    [Authorize]
    [Route("v1")]
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
        /// <summary>
        /// Creates new invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Patch invoice
        /// </summary>
        /// <param name="id"></param>
        /// <param name="invoice"></param>
        /// <returns></returns>
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
                var operations = ConvertRecord2Patch(dbInvoice.Data, invoice);
                var result = await client.InvoicePatchAsync(id, operations);
                return result.Data;
            }
        }

        /// <summary>
        /// Matches payment with the invoice
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("payment/{invoiceId}")]
        public async Task<ActionResult<OpenApiClient.Invoice>> MatchPaymentWithInvoice(string invoiceId, [FromBody] OpenApiClient.PaymentItem payment)
        {
            httpClient.PassHeaders(Request);

            var invoice = await client.InvoiceGetByIdAsync(invoiceId);

            if (invoice == null) throw new Exception("Invoice not found");
            var dbInvoice =
                JsonConvert.DeserializeObject<OpenApiClient.InvoiceDBBase>(
                    JsonConvert.SerializeObject(invoice)
                );

            if (invoice?.Data.PaymentInfo.Status == "PAID")
            {
                throw new Exception("Invoice has been already paid");
            }

            invoice?.Data.PaymentInfo.Payments.Add(payment);

            foreach(var paymentMethod in invoice?.Data.PaymentMethods ?? [])
            {
                double sum = 0;
                var err = false;
                foreach(var paymentDetail in invoice?.Data.PaymentInfo.Payments ?? [])
                {
                    if (paymentDetail.Currency != paymentMethod.Currency) err = true;
                    if (paymentDetail.CurrencyId != paymentMethod.CurrencyId) err = true;
                    if (paymentDetail.Network != paymentMethod.Network) err = true;
                    if (paymentDetail.Account != paymentMethod.Account) err = true;
                    sum += paymentDetail.GrossAmount ?? 0;
                }

                if (!err)
                {
                    if(sum == paymentMethod.GrossAmount)
                    {
                        invoice.Data.PaymentInfo.Status = "PAID";
                    }
                    else if(sum > 0)
                    {
                        if (invoice?.Data.PaymentInfo.Status != "PAID")
                        {
                            invoice.Data.PaymentInfo.Status = "PARTIALPAID";
                        }
                    }
                }
            }

            var operations = ConvertRecord2Patch(dbInvoice?.Data, invoice?.Data);

            var result = await client.InvoicePatchAsync(invoiceId, operations);
            return result.Data;
        }

        /// <summary>
        /// Creates new invoice with specific ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="invoice"></param>
        /// <returns></returns>
        [HttpPut("invoice/{id}")]
        public async Task<ActionResult<OpenApiClient.Invoice>> PutInvoice(string id, [FromBody] OpenApiClient.Invoice invoice)
        {
            httpClient.PassHeaders(Request);
            var dbInvoice = await client.InvoiceUpsertAsync(id, invoice);
            if (dbInvoice == null || dbInvoice.Data == null)
            {
                return NotFound();
            }
            else
            {
                var result = await client.InvoiceGetByIdAsync(id);
                return result.Data;
            }
        }
        /// <summary>
        /// Deletes invoice
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Returns invoice by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        /// List invoices
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet("invoice")]
        public async Task<ActionResult<IEnumerable<OpenApiClient.InvoiceDBBase>>> GetInvoices([FromQuery] int? offset = 0, [FromQuery] int? limit = 10, [FromQuery] string? sort = null)
        {
            httpClient.PassHeaders(Request);
            //var queryJson = new
            //{
            //    size = limit,
            //    from = offset,
            //    query = new
            //    {
            //        match_all = new { }
            //    }
            //};
            //string query = JsonConvert.SerializeObject(queryJson);
            var invoiceList = await client.InvoiceGetAsync(offset, limit, "*", sort);
            //var invoiceList = await client.InvoiceElasticQueryAsync(query);
            return invoiceList.Results.ToList();
        }
        /// <summary>
        /// Converts Model.NewInvoice model to OpenApiClient.Invoice? original path operations
        /// </summary>
        /// <param name="original"></param>
        /// <param name="invoice"></param>
        /// <returns></returns>
        private List<OpenApiClient.InvoiceOperation> ConvertRecord2Patch(OpenApiClient.Invoice? original, Model.NewInvoice? invoice)
        {
            try
            {
                if (original == null || invoice == null) return [];
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
        /// <summary>
        /// Converts Model.NewInvoice model to OpenApiClient.Invoice? original path operations
        /// </summary>
        /// <param name="original"></param>
        /// <param name="invoice"></param>
        /// <returns></returns>
        private List<OpenApiClient.InvoiceOperation> ConvertRecord2Patch(OpenApiClient.Invoice? original, OpenApiClient.Invoice? invoice)
        {
            try
            {
                if (original == null || invoice == null) return [];
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