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
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace TestALedgerBFFApi
{
    public class BFFControllerTests
    {
        private BFFController controller;
        [SetUp]
        public void Setup()
        {

            var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = new Mock<ILogger<BFFController>>();

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

            controller = new BFFController(logger.Object, mockOptions, mockOptionsBFF);

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
        private IOptionsMonitor<T> GetOptionsMonitor<T>(T appConfig)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<T>>();
            optionsMonitorMock.Setup(o => o.CurrentValue).Returns(appConfig);
            return optionsMonitorMock.Object;
        }

        [Test]
        public async Task TestIssueInvoice()
        {
            var prodInvoice = "65cde9fe-03db-4eeb-8ba1-b06b4270819c";
            var testInvoice = "bc896441-6f46-4e6c-af3a-b8661efd10bb";
            var invoice = await controller.PDF(testInvoice);
            Assert.IsNotNull(invoice);
        }
    }
}