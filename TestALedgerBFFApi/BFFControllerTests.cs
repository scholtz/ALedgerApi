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

            controller = new BFFController(logger.Object, mockOptions);

            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            mockRequest.Setup(x => x.Headers.Authorization).Returns("SigTx gqNzaWfEQHOzlUxzrk/3BWvhUKFiKo1AoUCfa4cDLwD7qvtJ6VMGmXfMwhDGeFU0F48weKAyBM5UORoi0vS7wMgd/73cHAejdHhuiaNmZWXNA+iiZnbOAcVMFqNnZW6sdGVzdG5ldC12MS4womdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4BxU/+pG5vdGXEDUFMZWRnZXIjYXJjMTSjcmN2xCCQjuXPPHXM7wbxO69McY2dwOQYXR1N+0dfAE3yvdjPoqNzbmTEIJCO5c88dczvBvE7r0xxjZ3A5BhdHU37R18ATfK92M+ipHR5cGWjcGF5");

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
        }
        private IOptionsMonitor<ObjectStorage> GetOptionsMonitor(ObjectStorage appConfig)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<ObjectStorage>>();
            optionsMonitorMock.Setup(o => o.CurrentValue).Returns(appConfig);
            return optionsMonitorMock.Object;
        }

        [Test]
        public async Task TestIssueInvoice()
        {
            var invoice = await controller.PDF("bc896441-6f46-4e6c-af3a-b8661efd10bb");
            Assert.IsNotNull(invoice);
        }
    }
}