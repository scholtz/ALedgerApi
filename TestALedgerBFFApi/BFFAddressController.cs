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
    public class AddressControllerTests
    {
        private AddressController controller;
        [SetUp]
        public void Setup()
        {

            var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = new Mock<ILogger<AddressController>>();

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

            controller = new AddressController(logger.Object, mockOptions, mockOptionsBFF);

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
        public async Task AddressCreate()
        {
            var newAddress = new NewAddress
            {
                City = "Praha",
                Country = "Èeská republika",
                CountryCode = "CZ",
                State = "CZ",
                Street = "Ulice 11",
                StreetLine2 = "Ulice 22",
                ZipCode = "11100"
            };
            var address = await controller.NewAddress(newAddress);
            Assert.IsNotNull(address);
        }

        [Test]
        public async Task AddressPatch()
        {
            var newAddress = new NewAddress
            {
                City = "Praha",
                Country = "Èeská republika",
                CountryCode = "CZ",
                State = "CZ",
                Street = "Ulice 11",
                StreetLine2 = "Ulice 22",
                ZipCode = "11100"
            };
            var address = await controller.NewAddress(newAddress);
            Assert.IsNotNull(address);
            Assert.IsNotNull(address?.Value);
            newAddress.City = "Brno";
            var result = await controller.PatchAddress(address.Value.Id, newAddress);
            Assert.IsNotNull(result?.Value);
            Assert.AreEqual(result?.Value?.City, "Brno");
        }

        [Test]
        public async Task AddressDelete()
        {
            var newAddress = new NewAddress
            {
                City = "Praha",
                Country = "Èeská republika",
                CountryCode = "CZ",
                State = "CZ",
                Street = "Ulice 11",
                StreetLine2 = "Ulice 22",
                ZipCode = "11100"
            };
            var address = await controller.NewAddress(newAddress);
            Assert.IsNotNull(address);
            Assert.IsNotNull(address.Value);
            var addressDelete = await controller.DeleteAddress(address.Value.Id);
            Assert.IsNotNull(address);
            Assert.IsNotNull(addressDelete?.Value);
            try
            {
                var addressGet = await controller.GetAddress(address.Value.Id);
                Assert.IsNull(addressGet);
            }
            catch (OpenApiClient.ApiException ex)
            {
                Assert.AreEqual(ex?.StatusCode, 204);
            }
        }

        [Test]
        public async Task AddressGetById()
        {
            var newAddress = new NewAddress
            {
                City = "Praha",
                Country = "Èeská republika",
                CountryCode = "CZ",
                State = "CZ",
                Street = "Ulice 11",
                StreetLine2 = "Ulice 22",
                ZipCode = "11100"
            };
            var address = await controller.NewAddress(newAddress);
            Assert.IsNotNull(address);
            Assert.IsNotNull(address.Value);
            var addressGet = await controller.GetAddress(address.Value.Id);
            Assert.IsNotNull(addressGet);
        }

        [Test]
        public async Task AddressGet()
        {
            var newAddress1 = new NewAddress
            {
                City = "Praha",
                Country = "Èeská republika",
                CountryCode = "CZ",
                State = "CZ",
                Street = "Ulice 11",
                StreetLine2 = "Ulice 22",
                ZipCode = "11100"
            };
            var address1 = await controller.NewAddress(newAddress1);
            Assert.IsNotNull(address1);
            Assert.IsNotNull(address1.Value);

            var newAddress2 = new NewAddress
            {
                City = "Praha",
                Country = "Èeská republika",
                CountryCode = "CZ",
                State = "CZ",
                Street = "Ulice 11",
                StreetLine2 = "Ulice 22",
                ZipCode = "11100"
            };
            var address2 = await controller.NewAddress(newAddress2);
            Assert.IsNotNull(address2);
            Assert.IsNotNull(address2.Value);

            var addressGet = await controller.GetAddresses(0, 2, null, null);
            Assert.IsNotNull(addressGet);
            Assert.IsNotNull(addressGet.Value);
            Assert.AreEqual(addressGet.Value.Count(), 2);
        }
    }
}