using ALedgerBFFApi.Controllers;
using ALedgerBFFApi.Model;
using ALedgerBFFApi.Model.Options;
using Algorand;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace TestALedgerBFFApi
{
    public class InvoiceControllerTests
    {
        private InvoiceController controller;
        [SetUp]
        public void Setup()
        {

            var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = new Mock<ILogger<InvoiceController>>();

            IOptionsMonitor<ObjectStorage> mockOptions = GetOptionsMonitor(new ObjectStorage()
            {
                Type = "FILE",
                Bucket = "Data",
            });
            IOptionsMonitor<BFF> mockOptionsBFF = GetOptionsMonitor(new BFF()
            {
                //DataServer = "https://ledger-data-api.h2.scholtz.sk",
                DataServer = "https://localhost:44375/",
            });

            controller = new InvoiceController(logger.Object, mockOptions, mockOptionsBFF);

            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            var prodAuth = "";
            var testAuth = "SigTx gqNzaWfEQHOzlUxzrk/3BWvhUKFiKo1AoUCfa4cDLwD7qvtJ6VMGmXfMwhDGeFU0F48weKAyBM5UORoi0vS7wMgd/73cHAejdHhuiaNmZWXNA+iiZnbOAcVMFqNnZW6sdGVzdG5ldC12MS4womdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4BxU/+pG5vdGXEDUFMZWRnZXIjYXJjMTSjcmN2xCCQjuXPPHXM7wbxO69McY2dwOQYXR1N+0dfAE3yvdjPoqNzbmTEIJCO5c88dczvBvE7r0xxjZ3A5BhdHU37R18ATfK92M+ipHR5cGWjcGF5";
            mockRequest.Setup(x => x.Headers.Authorization).Returns(testAuth);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
        }
        private IOptionsMonitor<T> GetOptionsMonitor<T>(T appConfig)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<T>>();
            optionsMonitorMock.Setup(o => o.CurrentValue).Returns(appConfig);
            return optionsMonitorMock.Object;
        }

        [Test]
        public async Task InvoiceCreate()
        {
            var newInvoice = new NewInvoice
            {
                Currency = "USD",
                DateDelivery = DateTimeOffset.Now,
                DateDue = DateTimeOffset.Now,
                DateIssue = DateTimeOffset.Now,
                InvoiceNumber = "00000000001",
                InvoiceNumberNum = 1,
                InvoiceType = "SELL",
                IsDraft = true,
                Items = new List<InvoiceItem> 
                {  
                  new InvoiceItem
                  {
                             Discount = 0,
                             GrossAmount = 0,
                             ItemText = string.Empty,
                             NetAmount = 0,
                             Quantity = 0,
                             TaxPercent = 0,
                             Unit = "Piece",
                             UnitPrice = 0
                  }                       
                }.ToArray(),  
                NoteAfterItems = string.Empty,
                NoteBeforeItems = string.Empty,
                PayableInDays = 0,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Account = "678678678",
                        Currency = "USD",
                        CurrencyId = "",
                        GrossAmount = 20,
                        Network = string.Empty
                    }
                }.ToArray(),
                PersonIdIssuer = string.Empty,
                PersonIdReceiver = string.Empty,
                Summary = new List<InvoiceSummaryInCurrency>
                {
                    new InvoiceSummaryInCurrency
                    {
                        Currency = "USD",
                        GrossAmount= 0,
                        NetAmount = 0,
                        Rate = 0,
                        RateCurrencies = "USD",
                        RateNote = string.Empty,
                        TaxAmount = 0
                    }
                }.ToArray()               
            };
            var invoice = await controller.NewInvoice(newInvoice);
            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice?.Value);
        }

        [Test]
        public async Task InvoicePatch()
        {
            var newInvoice = new NewInvoice
            {
                Currency = "USD",
                DateDelivery = DateTimeOffset.Now,
                DateDue = DateTimeOffset.Now,
                DateIssue = DateTimeOffset.Now,
                InvoiceNumber = "00000000001",
                InvoiceNumberNum = 1,
                InvoiceType = "SELL",
                IsDraft = true,
                Items = new List<InvoiceItem>
                {
                  new InvoiceItem
                  {
                             Discount = 0,
                             GrossAmount = 0,
                             ItemText = string.Empty,
                             NetAmount = 0,
                             Quantity = 0,
                             TaxPercent = 0,
                             Unit = "Piece",
                             UnitPrice = 0
                  }
                }.ToArray(),
                NoteAfterItems = string.Empty,
                NoteBeforeItems = string.Empty,
                PayableInDays = 0,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Account = "678678678",
                        Currency = "USD",
                        CurrencyId = "",
                        GrossAmount = 20,
                        Network = string.Empty
                    }
                }.ToArray(),
                PersonIdIssuer = string.Empty,
                PersonIdReceiver = string.Empty,
                Summary = new List<InvoiceSummaryInCurrency>
                {
                    new InvoiceSummaryInCurrency
                    {
                        Currency = "USD",
                        GrossAmount= 0,
                        NetAmount = 0,
                        Rate = 0,
                        RateCurrencies = "USD",
                        RateNote = string.Empty,
                        TaxAmount = 0
                    }
                }.ToArray()
            };
            var invoice = await controller.NewInvoice(newInvoice);
            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice?.Value);

            newInvoice.Currency = "EUR";
            newInvoice.InvoiceNumber = "00000000002";
            var result = await controller.PatchInvoice(invoice.Value.Id, newInvoice);
            Assert.IsNotNull(result?.Value);
            Assert.AreEqual(result?.Value?.Currency, "EUR");
            Assert.AreEqual(result?.Value?.InvoiceNumber, "00000000002");
        }

        [Test]
        public async Task InvoiceDelete()
        {
            var newInvoice = new NewInvoice
            {
                Currency = "USD",
                DateDelivery = DateTimeOffset.Now,
                DateDue = DateTimeOffset.Now,
                DateIssue = DateTimeOffset.Now,
                InvoiceNumber = "00000000001",
                InvoiceNumberNum = 1,
                InvoiceType = "SELL",
                IsDraft = true,
                Items = new List<InvoiceItem>
                {
                  new InvoiceItem
                  {
                             Discount = 0,
                             GrossAmount = 0,
                             ItemText = string.Empty,
                             NetAmount = 0,
                             Quantity = 0,
                             TaxPercent = 0,
                             Unit = "Piece",
                             UnitPrice = 0
                  }
                }.ToArray(),
                NoteAfterItems = string.Empty,
                NoteBeforeItems = string.Empty,
                PayableInDays = 0,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Account = "678678678",
                        Currency = "USD",
                        CurrencyId = "",
                        GrossAmount = 20,
                        Network = string.Empty
                    }
                }.ToArray(),
                PersonIdIssuer = string.Empty,
                PersonIdReceiver = string.Empty,
                Summary = new List<InvoiceSummaryInCurrency>
                {
                    new InvoiceSummaryInCurrency
                    {
                        Currency = "USD",
                        GrossAmount= 0,
                        NetAmount = 0,
                        Rate = 0,
                        RateCurrencies = "USD",
                        RateNote = string.Empty,
                        TaxAmount = 0
                    }
                }.ToArray()
            };
            var invoice = await controller.NewInvoice(newInvoice);
            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice?.Value);
            var invoiceDelete = await controller.DeleteInvoice(invoice.Value.Id);
            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoiceDelete?.Value);
            try
            {
                var invoiceGet = await controller.GetInvoice(invoice.Value.Id);
                Assert.IsNull(invoiceGet);
            }
            catch (OpenApiClient.ApiException ex)
            {
                Assert.AreEqual(ex?.StatusCode, 204);
            }
        }

        [Test]
        public async Task InvoiceGetById()
        {
            var newInvoice = new NewInvoice
            {
                Currency = "USD",
                DateDelivery = DateTimeOffset.Now,
                DateDue = DateTimeOffset.Now,
                DateIssue = DateTimeOffset.Now,
                InvoiceNumber = "00000000001",
                InvoiceNumberNum = 1,
                InvoiceType = "SELL",
                IsDraft = true,
                Items = new List<InvoiceItem>
                {
                  new InvoiceItem
                  {
                             Discount = 0,
                             GrossAmount = 0,
                             ItemText = string.Empty,
                             NetAmount = 0,
                             Quantity = 0,
                             TaxPercent = 0,
                             Unit = "Piece",
                             UnitPrice = 0
                  }
                }.ToArray(),
                NoteAfterItems = string.Empty,
                NoteBeforeItems = string.Empty,
                PayableInDays = 0,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Account = "678678678",
                        Currency = "USD",
                        CurrencyId = "",
                        GrossAmount = 20,
                        Network = string.Empty
                    }
                }.ToArray(),
                PersonIdIssuer = string.Empty,
                PersonIdReceiver = string.Empty,
                Summary = new List<InvoiceSummaryInCurrency>
                {
                    new InvoiceSummaryInCurrency
                    {
                        Currency = "USD",
                        GrossAmount= 0,
                        NetAmount = 0,
                        Rate = 0,
                        RateCurrencies = "USD",
                        RateNote = string.Empty,
                        TaxAmount = 0
                    }
                }.ToArray()
            };
            var invoice = await controller.NewInvoice(newInvoice);
            Assert.IsNotNull(invoice);
            Assert.IsNotNull(invoice?.Value);
            var invoiceGet = await controller.GetInvoice(invoice.Value.Id);
            Assert.IsNotNull(invoiceGet);
        }

        [Test]
        public async Task InvoiceGet()
        {
            var newInvoice1 = new NewInvoice
            {
                Currency = "USD",
                DateDelivery = DateTimeOffset.Now,
                DateDue = DateTimeOffset.Now,
                DateIssue = DateTimeOffset.Now,
                InvoiceNumber = "00000000001",
                InvoiceNumberNum = 1,
                InvoiceType = "SELL",
                IsDraft = true,
                Items = new List<InvoiceItem>
                {
                  new InvoiceItem
                  {
                             Discount = 0,
                             GrossAmount = 0,
                             ItemText = string.Empty,
                             NetAmount = 0,
                             Quantity = 0,
                             TaxPercent = 0,
                             Unit = "Piece",
                             UnitPrice = 0
                  }
                }.ToArray(),
                NoteAfterItems = string.Empty,
                NoteBeforeItems = string.Empty,
                PayableInDays = 0,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Account = "678678678",
                        Currency = "USD",
                        CurrencyId = "",
                        GrossAmount = 20,
                        Network = string.Empty
                    }
                }.ToArray(),
                PersonIdIssuer = string.Empty,
                PersonIdReceiver = string.Empty,
                Summary = new List<InvoiceSummaryInCurrency>
                {
                    new InvoiceSummaryInCurrency
                    {
                        Currency = "USD",
                        GrossAmount= 0,
                        NetAmount = 0,
                        Rate = 0,
                        RateCurrencies = "USD",
                        RateNote = string.Empty,
                        TaxAmount = 0
                    }
                }.ToArray()
            };
            var invoice1 = await controller.NewInvoice(newInvoice1);
            Assert.IsNotNull(invoice1);
            Assert.IsNotNull(invoice1?.Value);

            var newInvoice2 = new NewInvoice
            {
                Currency = "USD",
                DateDelivery = DateTimeOffset.Now,
                DateDue = DateTimeOffset.Now,
                DateIssue = DateTimeOffset.Now,
                InvoiceNumber = "00000000002",
                InvoiceNumberNum = 1,
                InvoiceType = "SELL",
                IsDraft = true,
                Items = new List<InvoiceItem>
                {
                  new InvoiceItem
                  {
                             Discount = 0,
                             GrossAmount = 0,
                             ItemText = string.Empty,
                             NetAmount = 0,
                             Quantity = 0,
                             TaxPercent = 0,
                             Unit = "Piece",
                             UnitPrice = 0
                  }
                }.ToArray(),
                NoteAfterItems = string.Empty,
                NoteBeforeItems = string.Empty,
                PayableInDays = 0,
                PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Account = "678678678",
                        Currency = "USD",
                        CurrencyId = "",
                        GrossAmount = 20,
                        Network = string.Empty
                    }
                }.ToArray(),
                PersonIdIssuer = string.Empty,
                PersonIdReceiver = string.Empty,
                Summary = new List<InvoiceSummaryInCurrency>
                {
                    new InvoiceSummaryInCurrency
                    {
                        Currency = "USD",
                        GrossAmount= 0,
                        NetAmount = 0,
                        Rate = 0,
                        RateCurrencies = "USD",
                        RateNote = string.Empty,
                        TaxAmount = 0
                    }
                }.ToArray()
            };
            var invoice2 = await controller.NewInvoice(newInvoice2);
            Assert.IsNotNull(invoice2);
            Assert.IsNotNull(invoice2?.Value);

            var invoiceGet = await controller.GetInvoices(0, 2, null, null);
            Assert.IsNotNull(invoiceGet);
            Assert.IsNotNull(invoiceGet.Value);
            Assert.AreEqual(invoiceGet.Value.Count(), 2);
        }
    }
}