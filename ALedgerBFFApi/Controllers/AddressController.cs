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
    public class AddressController : Controller
    {
        private readonly OpenApiClient.Client client;
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IOptionsMonitor<ObjectStorage> objectStorageConfig;
        private readonly ILogger<AddressController> logger;
        public AddressController(ILogger<AddressController> logger, IOptionsMonitor<ObjectStorage> objectStorageConfig, IOptionsMonitor<BFF> bffConfig) : base()
        {
            this.logger = logger;
            this.objectStorageConfig = objectStorageConfig;
            client = new OpenApiClient.Client(bffConfig.CurrentValue.DataServer, httpClient);

        }

        [HttpPost]
        public async Task<OpenApiClient.Address> NewAddress([FromBody] Model.NewAddress address)
        {
            httpClient.PassHeaders(Request);
            var dbAddress = new OpenApiClient.Address
            {
                City = address.City,
                Country = address.Country,
                CountryCode = address.CountryCode,
                State = address.State,
                Street = address.Street,
                StreetLine2 = address.StreetLine2,
                ZipCode = address.ZipCode              
            };
            var result = await client.PostAddressAsync(dbAddress);
            return result.Data;
        }
    }
}