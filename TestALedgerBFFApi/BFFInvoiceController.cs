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
            //var newInvoice = new NewInvoice
            //{
            //    //TODO   
            //};
            //var invoice = await controller.NewInvoice(newInvoice);
            //Assert.IsNotNull(invoice);

            Assert.Pass();
        }

        [Test]
        public async Task InvoicePatch()
        {
            //var newInvoice = new NewInvoice
            //{
            //    //TODO 
            //};
            //var invoice = await controller.NewInvoice(newInvoice);
            //Assert.IsNotNull(invoice);
            //Assert.IsNotNull(invoice?.Value);

            Assert.Pass();
        }

        [Test]
        public async Task InvoiceDelete()
        {
            //TODO

            Assert.Pass();
        }

        [Test]
        public async Task InvoiceGetById()
        {
            //TODO

            Assert.Pass();
        }

        [Test]
        public async Task InvoiceGet()
        {
            //TODO

            Assert.Pass();
        }
    }
}