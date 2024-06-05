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
using System.Threading;
using System.Web.Http.Controllers;

namespace TestALedgerBFFApi
{
    public class PersonControllerTests
    {
        private PersonController controller;
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


            controller = scope.ServiceProvider.GetService(typeof(PersonController)) as PersonController ?? throw new Exception("PersonController not initialized");

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
        public async Task PersonCreate()
        {
            var newPerson = new NewPerson
            {
                 AddressId = "fc5a89d3-957d-4142-9aa4-24b02f9a4f18",
                 //CompanyId = TODO,
                 BusinessName = "Business name",
                 Email = "email@email.cz",
                //CompanyTaxId = TODO,
                //CompanyVATId = TODO,
                FirstName = "First",
                LastName = "Last",
                Phone = "00000000",
                SignatureUrl = "www.TEST.com",
                Address = new BFFAddress
                {
                    City = "Praha",
                    Country = "?eská republika",
                    CountryCode = "CZ",
                    State = "CZ",
                    Street = "Ulice 11",
                    StreetLine2 = "Ulice 22",
                    ZipCode = "11100"
                }
            };
            var person = await controller.NewPerson(newPerson);
            Assert.IsNotNull(person);
        }

        [Test]
        public async Task PersonPatch()
        {
            var newPerson = new NewPerson
            {
                AddressId = "fc5a89d3-957d-4142-9aa4-24b02f9a4f18",
                //CompanyId = TODO,
                BusinessName = "Business name",
                Email = "email@email.cz",
                //CompanyTaxId = TODO,
                //CompanyVATId = TODO,
                FirstName = "First",
                LastName = "Last",
                Phone = "00000000",
                SignatureUrl = "www.TEST.com",
                Address = new BFFAddress
                {
                    City = "Praha",
                    Country = "?eská republika",
                    CountryCode = "CZ",
                    State = "CZ",
                    Street = "Ulice 11",
                    StreetLine2 = "Ulice 22",
                    ZipCode = "11100"
                }
            };
            var person = await controller.NewPerson(newPerson);
            Assert.IsNotNull(person);
            Assert.IsNotNull(person?.Value);
            newPerson.FirstName = "First2";
            newPerson.LastName = "Last2";
            var result = await controller.PatchPerson(person.Value.Id, newPerson);
            Assert.IsNotNull(result?.Value);
            Assert.AreEqual(result?.Value?.FirstName, "First2");
            Assert.AreEqual(result?.Value?.LastName, "Last2");
        }

        [Test]
        public async Task PersonDelete()
        {
            var newPerson = new NewPerson
            {
                AddressId = "fc5a89d3-957d-4142-9aa4-24b02f9a4f18",
                //CompanyId = TODO,
                BusinessName = "Business name",
                Email = "email@email.cz",
                //CompanyTaxId = TODO,
                //CompanyVATId = TODO,
                FirstName = "First",
                LastName = "Last",
                Phone = "00000000",
                SignatureUrl = "www.TEST.com",
                Address = new BFFAddress
                {
                    City = "Praha",
                    Country = "?eská republika",
                    CountryCode = "CZ",
                    State = "CZ",
                    Street = "Ulice 11",
                    StreetLine2 = "Ulice 22",
                    ZipCode = "11100"
                }
            };
            var person = await controller.NewPerson(newPerson);
            Assert.IsNotNull(person);
            Assert.IsNotNull(person.Value);
            var personDelete = await controller.DeletePerson(person.Value.Id);
            Assert.IsNotNull(person);
            Assert.IsNotNull(personDelete?.Value);
            try
            {
                var personGet = await controller.GetPerson(person.Value.Id);
                Assert.IsNull(personGet);
            }
            catch (OpenApiClient.ApiException ex)
            {
                Assert.AreEqual(ex?.StatusCode, 204);
            }
        }

        [Test]
        public async Task PersonGetById()
        {
            var newPerson = new NewPerson
            {
                AddressId = "fc5a89d3-957d-4142-9aa4-24b02f9a4f18",
                //CompanyId = TODO,
                BusinessName = "Business name",
                Email = "email@email.cz",
                //CompanyTaxId = TODO,
                //CompanyVATId = TODO,
                FirstName = "First",
                LastName = "Last",
                Phone = "00000000",
                SignatureUrl = "www.TEST.com",
                Address = new BFFAddress
                {
                    City = "Praha",
                    Country = "?eská republika",
                    CountryCode = "CZ",
                    State = "CZ",
                    Street = "Ulice 11",
                    StreetLine2 = "Ulice 22",
                    ZipCode = "11100"
                }
            };
            var person = await controller.NewPerson(newPerson);
            Assert.IsNotNull(person);
            Assert.IsNotNull(person.Value);
            var personGet = await controller.GetPerson(person.Value.Id);
            Assert.IsNotNull(personGet);
        }

        [Test]
        public async Task PersonGet()
        {
            var newPerson1 = new NewPerson
            {
                AddressId = "fc5a89d3-957d-4142-9aa4-24b02f9a4f00",
                //CompanyId = TODO,
                BusinessName = "Business name",
                Email = "email@email.cz",
                //CompanyTaxId = TODO,
                //CompanyVATId = TODO,
                FirstName = "First",
                LastName = "Last",
                Phone = "00000000",
                SignatureUrl = "www.TEST.com",
                Address = new BFFAddress
                {
                    City = "Praha",
                    Country = "?eská republika",
                    CountryCode = "CZ",
                    State = "CZ",
                    Street = "Ulice 11",
                    StreetLine2 = "Ulice 22",
                    ZipCode = "11100"
                }
            };
            var person1 = await controller.NewPerson(newPerson1);
            Assert.IsNotNull(person1);
            Assert.IsNotNull(person1.Value);

            var personGet = await controller.GetPersons(0, 2, null, null);
            Assert.IsNotNull(personGet);
            Assert.IsNotNull(personGet.Value);
            Assert.That(personGet.Value.Count(), Is.LessThanOrEqualTo(2));

            var newPerson2 = new NewPerson
            {
                AddressId = "61ec1f86-ea7d-4eae-923b-89cd7292f501",
                //CompanyId = TODO,
                BusinessName = "Business name 2",
                Email = "email2@email.cz",
                //CompanyTaxId = TODO,
                //CompanyVATId = TODO,
                FirstName = "First2",
                LastName = "Last2",
                Phone = "00000000",
                SignatureUrl = "www.TEST2.com",
                Address = new BFFAddress
                {
                    City = "Praha",
                    Country = "?eská republika",
                    CountryCode = "CZ",
                    State = "CZ",
                    Street = "Ulice 11",
                    StreetLine2 = "Ulice 22",
                    ZipCode = "11100"
                }
            };
            var person2 = await controller.NewPerson(newPerson2);
            Assert.IsNotNull(person2);
            Assert.IsNotNull(person2.Value);
          
            personGet = await controller.GetPersons(0, 2, null, null);
            Assert.IsNotNull(personGet);
            Assert.IsNotNull(personGet.Value);
            Assert.That(personGet.Value.Count(), Is.EqualTo(2));
        }
    }
}