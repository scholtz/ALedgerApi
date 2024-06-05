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
using System.IO;
using System.Text;

namespace ALedgerBFFApi.Controllers
{
    [Authorize]
    [Route("v1")]
    [ApiController]
    public class BFFController : Controller
    {
        private readonly OpenApiClient.Client client;
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IOptionsMonitor<ObjectStorage> objectStorageConfig;
        private readonly ILogger<BFFController> logger;
        public BFFController(ILogger<BFFController> logger, IOptionsMonitor<ObjectStorage> objectStorageConfig, IOptionsMonitor<BFF> bffConfig) : base()
        {
            this.logger = logger;
            this.objectStorageConfig = objectStorageConfig;
            client = new OpenApiClient.Client(bffConfig.CurrentValue.DataServer, httpClient);

        }


        [HttpGet("get-user")]
        public string? GetUser()
        {
            return User?.Identity?.Name;
        }
        [HttpGet("get-first-person")]
        public async Task<OpenApiClient.Person> GetPerson()
        {
            httpClient.PassHeaders(Request);
            var persons = await client.PersonGetAsync(0, 10, "*", "");
            return persons.Results.First().Data;
        }
        [HttpPost("issue-invoice")]
        public async Task<OpenApiClient.Person> IssueInvoice([FromBody] Model.NewInvoice newInvoice)
        {
            httpClient.PassHeaders(Request);
            var issuer = await client.PersonGetByIdAsync(newInvoice.PersonIdIssuer);
            var receiver = await client.PersonGetByIdAsync(newInvoice.PersonIdReceiver);
            var newDBInvoice = new OpenApiClient.Invoice();
            newDBInvoice.NoteBeforeItems = newInvoice.NoteBeforeItems;
            return receiver.Data;
        }
        [HttpGet("pdf/{invoiceId}")]
        public async Task<string> PDF([FromRoute] string invoiceId)
        {
            httpClient.PassHeaders(Request);

            OpenApiClient.InvoiceDBBase? invoice = null;
            try
            {
                invoice = await client.InvoiceGetByIdAsync(invoiceId);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load invoice"); }
            if(invoice?.Data.InvoiceNumber == null)
            {
                throw new Exception("Invoice not found");
            }
            OpenApiClient.PersonDBBase? issuer = null;
            try
            {
                if (invoice?.Data?.PersonIdIssuer == null) throw new Exception("Invoice does not have issuer");
                issuer = await client.PersonGetByIdAsync(invoice.Data.PersonIdIssuer);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load issuer"); }


            OpenApiClient.PersonDBBase? receiver = null;
            try
            {
                if (invoice?.Data?.PersonIdReceiver == null) throw new Exception("Invoice does not have receiver");
                receiver = await client.PersonGetByIdAsync(invoice.Data.PersonIdReceiver);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load receiver"); }


            OpenApiClient.AddressDBBase? issuerAddress = null;
            if (issuer?.Data?.Address != null)
            {
                issuerAddress = new OpenApiClient.AddressDBBase();
                issuerAddress.Data = issuer?.Data?.Address;
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(issuer?.Data?.AddressId)) throw new Exception("Issuer does not have address");
                    issuerAddress = await client.AddressGetByIdAsync(issuer.Data.AddressId);
                }
                catch (Exception ex) { logger.LogError(ex, "Unable to load issuer address"); }
            }

            OpenApiClient.AddressDBBase? receiverAddress = null;
            if (receiver?.Data?.Address != null)
            {
                receiverAddress = new OpenApiClient.AddressDBBase();
                receiverAddress.Data = receiver?.Data?.Address;
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(receiver?.Data?.AddressId)) throw new Exception("Receiver does not have address");
                    receiverAddress = await client.AddressGetByIdAsync(receiver.Data.AddressId);
                }
                catch (Exception ex) { logger.LogError(ex, "Unable to load receiver address"); }
            }

            OpenApiClient.InvoiceItemInvoiceItemDBBaseDBListBase? items = null;

            if (invoice?.Data?.Items != null)
            {
                items = new OpenApiClient.InvoiceItemInvoiceItemDBBaseDBListBase();
                items.Results = invoice.Data.Items.Select(i => new OpenApiClient.InvoiceItemDBBase
                {
                    Data = i
                }).ToList();
            }
            else
            {
                try
                {
                    items = await client.InvoiceItemGetAsync(0, 100, invoiceId, "");
                }
                catch (Exception ex) { logger.LogError(ex, "Unable to load receiver"); }
            }

            string source = System.IO.File.ReadAllText("Data/InvoiceTemplate.html");
            var template = Handlebars.Compile(source);

            var data = new
            {
                Invoice = invoice,
                Issuer = issuer,
                IssuerAddress = issuerAddress,
                Receiver = receiver,
                ReceiverAddress = receiverAddress,
                Items = items,
                HasManyPaymentMethods = invoice.Data.PaymentMethods.Count > 1,
                HasAnyRate = invoice.Data.Summary.FirstOrDefault(s=>s.Rate > 0) != null,
                HasAnyRateNotes = invoice.Data.Summary.FirstOrDefault(s => !string.IsNullOrEmpty(s.RateNote)) != null,
                Link = "https://ledger.a-wallet.net/invoice/"+invoice.Id
            };

            Handlebars.RegisterHelper("dateFormat", (output, context, data) =>
            {
                var format = data[0]?.ToString();
                var time = data[1] as DateTimeOffset?;
                if (!time.HasValue) return;
                output.WriteSafeString(time.Value.ToString(format));
            });

            Handlebars.RegisterHelper("numberFormat", (output, context, data) =>
            {
                var format = data[0]?.ToString();
                var value = data[1] as decimal?;
                if (value.HasValue)
                {
                    output.WriteSafeString(value.Value.ToString(format));
                    return;
                }

                if (decimal.TryParse(data[1]?.ToString(), out var num))
                {
                    output.WriteSafeString(num.ToString(format));
                    return;
                }


                output.WriteSafeString(0d.ToString(format));
                return;
            });

            var resultHtml = template(data);

            if (objectStorageConfig.CurrentValue.Type != "AWS")
            {
                var uploadToHtml = $"invoice/{invoice.Data.InvoiceNumber}-{invoiceId}.html";
                await objectStorageConfig.CurrentValue.Upload(uploadToHtml, Encoding.UTF8.GetBytes(resultHtml));
            }
            //invoice.Data.DateIssue
            using var memoryStream = new MemoryStream();
            var wProps = new WriterProperties();
            var pdfWriter = new PdfWriter(memoryStream, wProps);
            
            var pdfDocument = new PdfDocument(pdfWriter);
            pdfDocument.SetDefaultPageSize(PageSize.A4.Rotate());

            //document.html
            ConverterProperties converterProperties = new ConverterProperties();
            converterProperties.SetImmediateFlush(false);
            
            var document = iText.Html2pdf.HtmlConverter.ConvertToDocument(resultHtml, pdfDocument, converterProperties);
            document.Close();

            using var memoryStream2 = new MemoryStream();
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(memoryStream.ToArray())), new PdfWriter(memoryStream2));
            Document doc2 = new Document(pdfDoc);

            addPageNumbers(doc2);

            doc2.Close();

            var uploadTo = $"invoice/{invoice.Data.InvoiceNumber}-{invoiceId}.pdf";
            //uploadTo = Guid.NewGuid().ToString() + ".pdf";

            var ok = await objectStorageConfig.CurrentValue.Upload(uploadTo, memoryStream2.ToArray());
            if (!ok) throw new Exception("Error occured while processing pdf upload to storage");


            return $"https://{objectStorageConfig.CurrentValue.Bucket}.{objectStorageConfig.CurrentValue.Host}/{uploadTo}";
        }
        private void addPageNumbers(Document doc)
        {
            var totalPages = doc.GetPdfDocument().GetNumberOfPages();
            Style small = new Style();
            small.SetFontSize(8);
            small.SetFontColor(new iText.Kernel.Colors.DeviceGray(0.5f));
            for (int i = 1; i <= totalPages; i++)
            {
                // Write aligned text to the specified by parameters point
                doc.ShowTextAligned(new Paragraph(string.Format("Page {0} of {1}", i, totalPages)).AddStyle(small), 806, 559, i, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);
                //doc.ShowTextAligned(new Paragraph(string.Format("Page {0} of {1}", i, totalPages)).AddStyle(small), 100, 100, i, TextAlignment.LEFT, VerticalAlignment.TOP, 0);

            }
        }

    }
}