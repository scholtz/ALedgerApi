using ALedgerBFFApi.Controllers;
using ALedgerBFFApi.Model;
using ALedgerBFFApi.Model.Options;
using Algorand;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private IServiceScope? scope;
        [SetUp]
        public void Setup()
        {

            WebApplication appAPI = ALedgerApi.Program.CreateWebApplication("appsettings.api.json");
            Task.Run(() => appAPI.RunAsync(), cancellationToken.Token);
            WebApplication appBFF = ALedgerBFFApi.Program.CreateWebApplication("appsettings.bff.json");
            Task.Run(() => appBFF.RunAsync(), cancellationToken.Token);
            scope = appBFF.Services.CreateScope();

            controller = scope.ServiceProvider.GetService(typeof(InvoiceController)) as InvoiceController ?? throw new Exception("InvoiceController not initialized");

            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            var prodAuth = "";
            var testAuth = "SigTx gqNzaWfEQFjSeOHx3n/9Zp52VDcN4Qu+rBWz4thcWYG9dl21C0yd0bkBNaLOc9US9NptCuM50E6juX2xe7DARBewbhAcMAijdHhuiaNmZWXNA+iiZnbOAlEc16NnZW6sdGVzdG5ldC12MS4womdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4CUSC/pG5vdGXEFkJpYXRlY0FjY291bnRpbmcjQVJDMTSjcmN2xCB57A2sF+bF3WK1mt9cRThlADlMeEdTmlEo6qhAfVAyC6NzbmTEIHnsDawX5sXdYrWa31xFOGUAOUx4R1OaUSjqqEB9UDILpHR5cGWjcGF5";
            mockRequest.Setup(x => x.Headers.Authorization).Returns(testAuth);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
        }
        [TearDown]
        public void TearDown()
        {
            cancellationToken?.Dispose();
            scope?.Dispose();
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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
            Assert.That(invoice, Is.Not.Null);
            Assert.That(invoice?.Value, Is.Not.Null);

            newInvoice.Currency = "EUR";
            newInvoice.InvoiceNumber = "00000000002";
            var result = await controller.PatchInvoice(invoice.Value.Id, newInvoice);
            Assert.That(result?.Value, Is.Not.Null);
            Assert.That(result?.Value?.Currency, Is.EqualTo("EUR"));
            Assert.That(result?.Value?.InvoiceNumber, Is.EqualTo("00000000002"));


            var loadedInvoice = await controller.GetInvoice(invoice.Value.Id);
            Assert.That(loadedInvoice?.Value, Is.Not.Null);
            Assert.That(loadedInvoice?.Value?.Currency, Is.EqualTo("EUR"));
            Assert.That(loadedInvoice?.Value?.InvoiceNumber, Is.EqualTo("00000000002"));
        }

        [Test]
        public async Task InvoicePut()
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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
            Assert.That(invoice, Is.Not.Null);
            Assert.That(invoice?.Value, Is.Not.Null);

            newInvoice.Currency = "EUR";
            newInvoice.InvoiceNumber = "00000000002";
            await Task.Delay(10000);
            var loadedInvoice = await controller.GetInvoice(invoice.Value.Id);
            Assert.That(loadedInvoice?.Value, Is.Not.Null);
            Assert.That(loadedInvoice?.Value?.Currency, Is.EqualTo("USD"));
            Assert.That(loadedInvoice?.Value?.InvoiceNumber, Is.EqualTo("00000000001"));


            var result = await controller.PutInvoice(invoice.Value.Id, newInvoice.ToOpenApi());
            Assert.That(result?.Value, Is.Not.Null);
            Assert.That(result?.Value?.Currency, Is.EqualTo("EUR"));
            Assert.That(result?.Value?.InvoiceNumber, Is.EqualTo("00000000002"));
            await Task.Delay(100);
            loadedInvoice = await controller.GetInvoice(invoice.Value.Id);
            Assert.That(loadedInvoice?.Value, Is.Not.Null);
            Assert.That(loadedInvoice?.Value?.Currency, Is.EqualTo("EUR"));
            Assert.That(loadedInvoice?.Value?.InvoiceNumber, Is.EqualTo("00000000002"));

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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
                Items = new List<BFFInvoiceItem>
                {
                  new BFFInvoiceItem
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
                PaymentMethods = new List<BFFPaymentMethod>
                {
                    new BFFPaymentMethod
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
                Summary = new List<BFFInvoiceSummaryInCurrency>
                {
                    new BFFInvoiceSummaryInCurrency
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
            Assert.That(invoice2, Is.Not.Null);
            Assert.That(invoice2?.Value, Is.Not.Null);

            var invoiceGet = await controller.GetInvoices(offset: 0, limit: 2);
            Assert.That(invoiceGet, Is.Not.Null);
            Assert.That(invoiceGet.Value, Is.Not.Null);
            Assert.That(invoiceGet.Value.Count(), Is.LessThanOrEqualTo(2));
        }
    }
}