using ALedgerBFFApi.Controllers;
using ALedgerBFFApi.Model.Options;
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
                DataServer = "https://ledger-data-api.h2.scholtz.sk",
            });

            controller = new BFFController(logger.Object, mockOptions, mockOptionsBFF);

            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            var prodAuth = "SigTx gqNzaWfEQPD55jWXtXAD2BAGRQfZbFZzs8J/c/7+fZY2K67DIP0YGCkUrPIaTX2C8ZyinHjQeCq7Zuq4C+3v+WhVmerqJACjdHhuiaNmZWXNA+iiZnbOAjJekqNnZW6sbWFpbm5ldC12MS4womdoxCDAYcTY/B293tLXYEvkVo4/bQQZh6w3veS2ILWrOSSK36Jsds4CMmJ6pG5vdGXEDUFMZWRnZXIjYXJjMTSjcmN2xCB57A2sF+bF3WK1mt9cRThlADlMeEdTmlEo6qhAfVAyC6NzbmTEIHnsDawX5sXdYrWa31xFOGUAOUx4R1OaUSjqqEB9UDILpHR5cGWjcGF5";
            var testAuth = "SigTx gqNzaWfEQHOzlUxzrk/3BWvhUKFiKo1AoUCfa4cDLwD7qvtJ6VMGmXfMwhDGeFU0F48weKAyBM5UORoi0vS7wMgd/73cHAejdHhuiaNmZWXNA+iiZnbOAcVMFqNnZW6sdGVzdG5ldC12MS4womdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4BxU/+pG5vdGXEDUFMZWRnZXIjYXJjMTSjcmN2xCCQjuXPPHXM7wbxO69McY2dwOQYXR1N+0dfAE3yvdjPoqNzbmTEIJCO5c88dczvBvE7r0xxjZ3A5BhdHU37R18ATfK92M+ipHR5cGWjcGF5";
            mockRequest.Setup(x => x.Headers.Authorization).Returns(prodAuth);

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
            var testInvoice = "b7f2620c-5613-4177-9614-5d44a7c70d12";
            var invoice = await controller.PDF(prodInvoice);
            Assert.IsNotNull(invoice);
        }
    }
}