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
                DataServer = "https://ledger-data-api.h2.scholtz.sk",
                // DataServer = "https://localhost:44375/",
            });

            controller = new BFFController(logger.Object, mockOptions, mockOptionsBFF);

            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            var prodAuth = "SigTx gqNzaWfEQJvEv8ykx7ofRMjZhEM/hr/rICUKIjBCh6sdRprYbByi3aOFVjzfD8nzqGLLBLKR1LN4zMGWZ3rDNhUnMqB1Ow6jdHhuiKJmds4CdLcKo2dlbqxtYWlubmV0LXYxLjCiZ2jEIMBhxNj8Hb3e0tdgS+RWjj9tBBmHrDe95LYgtas5JIrfomx2zgJ0uvKkbm90ZcQWQmlhdGVjQWNjb3VudGluZyNBUkMxNKNyY3bEIJCO5c88dczvBvE7r0xxjZ3A5BhdHU37R18ATfK92M+io3NuZMQgkI7lzzx1zO8G8TuvTHGNncDkGF0dTftHXwBN8r3Yz6KkdHlwZaNwYXk=";
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
            var prodInvoice = "0030609c-0ece-4299-8e8b-0ec5959eac2c";
            var invoice = await controller.PDF(prodInvoice);
            Assert.IsNotNull(invoice);
        }
    }
}