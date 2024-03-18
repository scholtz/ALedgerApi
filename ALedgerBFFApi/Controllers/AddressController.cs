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
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
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
        public async Task<ActionResult<OpenApiClient.AddressDBBase>> NewAddress([FromBody] Model.NewAddress address)
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

            var result = await client.AddressPostAsync(dbAddress);
            return result;
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<OpenApiClient.Address>> PatchAddress(string id, [FromBody] Model.NewAddress address)
        {
            httpClient.PassHeaders(Request);
            var dbAddress = await client.AddressGetByIdAsync(id);
            if (dbAddress == null || dbAddress.Data == null)
            {
                return NotFound();
            }
            else
            {
                var operations = await ConvertRecord2Patch(dbAddress.Data, address);                
                var result = await client.AddressPatchAsync(id, operations);
                return result.Data;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<OpenApiClient.Address>> DeleteAddress(string id)
        {
            httpClient.PassHeaders(Request);
            var dbAddress = await client.AddressGetByIdAsync(id);
            if (dbAddress == null || dbAddress.Data == null)
            {
                return NotFound();
            }
            else
            {
                var result = await client.AddressDeleteAsync(id);
                return result.Data;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OpenApiClient.Address>> GetAddress(string id)
        {
            httpClient.PassHeaders(Request);
            var dbAddress = await client.AddressGetByIdAsync(id);
            if (dbAddress == null || dbAddress.Data == null)
            {
                return NotFound();
            }
            else
            {
                return dbAddress.Data;
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OpenApiClient.AddressDBBase>>> GetAddresses([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string query, [FromQuery] string sort)
        {
            httpClient.PassHeaders(Request);
            var addressList = await client.AddressGetAsync(offset, limit, query, sort);
            return addressList.Results.ToList();
        }

        private async Task<List<OpenApiClient.AddressOperation>> ConvertRecord2Patch(OpenApiClient.Address original, Model.NewAddress address)
        {
            try
            {
                var retOperations = new List<OpenApiClient.AddressOperation>(); 
                foreach (PropertyInfo propertyInfoOriginal in original.GetType().GetProperties())
                {
                    var origValue = JsonConvert.SerializeObject(propertyInfoOriginal.GetValue(original));
                    var propertyInfo = address.GetType().GetProperty(propertyInfoOriginal.Name);
                    if (propertyInfo == null) 
                    {
                        logger.LogError("Property name does not exist");
                        continue;
                    }
                    var updatedValue = JsonConvert.SerializeObject(propertyInfo?.GetValue(address));
                    if (
                        origValue == null && updatedValue != null ||
                        updatedValue == null && origValue != null ||
                        (origValue != null && !origValue.Equals(updatedValue)))
                    {
                        var op = new OpenApiClient.AddressOperation()
                        {
                            Op = "replace",
                            //OperationType = 
                            Path = propertyInfo.Name,
                            Value = propertyInfo.GetValue(address)
                        };
                        retOperations.Add(op);
                    }
                }

                return retOperations;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}