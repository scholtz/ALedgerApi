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
    [Route("v1/[controller]")]
    [ApiController]
    public class BFFController : Controller
    {
        private readonly OpenApiClient.Client client;
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IOptionsMonitor<ObjectStorage> objectStorageConfig;
        private readonly ILogger<BFFController> logger;
        public BFFController(ILogger<BFFController> logger, IOptionsMonitor<ObjectStorage> objectStorageConfig) : base()
        {
            this.logger = logger;
            this.objectStorageConfig = objectStorageConfig;
            client = new OpenApiClient.Client("https://ledger-data-api-dev.s4.a-wallet.net", httpClient);

            var authHeader = Request?.Headers?.Authorization.ToString()?.Replace("SigTx ", "");
            if (!string.IsNullOrEmpty(authHeader))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SigTx", authHeader);
            }
        }


        [HttpGet("GetUser")]
        public string? GetUser()
        {
            return User?.Identity?.Name;
        }
        [HttpGet("GetTop1")]
        public async Task<OpenApiClient.Person> GetPerson()
        {
            httpClient.PassHeaders(Request);
            var persons = await client.GetPersonAsync(0, 10, "*", "");
            return persons.Results.First().Data;
        }
        [HttpPost("IssueInvoice")]
        public async Task<OpenApiClient.Person> IssueInvoice([FromBody] Model.NewInvoice newInvoice)
        {
            httpClient.PassHeaders(Request);
            var issuer = await client.GetPersonByIdAsync(newInvoice.PersonIdIssuer);
            var receiver = await client.GetPersonByIdAsync(newInvoice.PersonIdReceiver);
            var newDBInvoice = new OpenApiClient.Invoice();
            newDBInvoice.NoteBeforeItems = newInvoice.NoteBeforeItems;
            return receiver.Data;
        }
        [HttpGet("PDF/{invoiceId}")]
        public async Task<string> PDF([FromRoute] string invoiceId)
        {
            httpClient.PassHeaders(Request);

            OpenApiClient.InvoiceDBBase? invoice = null;
            try
            {
                invoice = await client.GetInvoiceByIdAsync(invoiceId);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load invoice"); }

            OpenApiClient.PersonDBBase? issuer = null;
            try
            {
                if (invoice?.Data?.PersonIdIssuer == null) throw new Exception("Invoice does not have issuer");
                issuer = await client.GetPersonByIdAsync(invoice.Data.PersonIdIssuer);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load issuer"); }


            OpenApiClient.PersonDBBase? receiver = null;
            try
            {
                if (invoice?.Data?.PersonIdReceiver == null) throw new Exception("Invoice does not have receiver");
                receiver = await client.GetPersonByIdAsync(invoice.Data.PersonIdReceiver);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load receiver"); }


            OpenApiClient.AddressDBBase? issuerAddress = null;
            try
            {
                if (string.IsNullOrEmpty(issuer?.Data?.AddressId)) throw new Exception("Issuer does not have address");
                issuerAddress = await client.GetAddressByIdAsync(issuer.Data.AddressId);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load issuer address"); }

            OpenApiClient.AddressDBBase? receiverAddress = null;
            try
            {
                if (string.IsNullOrEmpty(receiver?.Data?.AddressId)) throw new Exception("Receiver does not have address");
                receiverAddress = await client.GetAddressByIdAsync(receiver.Data.AddressId);
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load receiver address"); }


            OpenApiClient.InvoiceItemInvoiceItemDBBaseDBListBase? items = null;
            try
            {
                items = await client.GetInvoiceitemAsync(0, 100, invoiceId, "");
            }
            catch (Exception ex) { logger.LogError(ex, "Unable to load receiver"); }

            string source = System.IO.File.ReadAllText("Data/InvoiceTemplate.html");
            var template = Handlebars.Compile(source);

            var data = new
            {
                Invoice = invoice,
                Issuer = issuer,
                IssuerAddress = issuerAddress,
                Receiver = receiver,
                ReceiverAddress = receiverAddress,
                Items = items
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
            addPageNumbers(document);
            document.Close();
            var uploadTo = $"invoice/{invoice.Data.InvoiceNumber}-{invoiceId}.pdf";
            //uploadTo = Guid.NewGuid().ToString() + ".pdf";

            var ok = await objectStorageConfig.CurrentValue.Upload(uploadTo, memoryStream.ToArray());
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
                doc.ShowTextAligned(new Paragraph(string.Format("Page {0} of {1}", i, totalPages)).AddStyle(small),
                                559, 806, i, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);
            }
        }

    }
}