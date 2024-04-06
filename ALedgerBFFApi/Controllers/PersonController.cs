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
    public class PersonController : Controller
    {
        private readonly OpenApiClient.Client client;
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IOptionsMonitor<ObjectStorage> objectStorageConfig;
        private readonly ILogger<PersonController> logger;
        public PersonController(ILogger<PersonController> logger, IOptionsMonitor<ObjectStorage> objectStorageConfig, IOptionsMonitor<BFF> bffConfig) : base()
        {
            this.logger = logger;
            this.objectStorageConfig = objectStorageConfig;
            client = new OpenApiClient.Client(bffConfig.CurrentValue.DataServer, httpClient);
        }

        [HttpPost("person")]
        public async Task<ActionResult<OpenApiClient.PersonDBBase>> NewPerson([FromBody] Model.NewPerson person)
        {
            httpClient.PassHeaders(Request);
            var dbPerson = new OpenApiClient.Person
            {
                AddressId = person.AddressId,
                BusinessName = person.BusinessName,
                CompanyId = person.CompanyId,
                CompanyTaxId = person.CompanyTaxId,
                CompanyVATId = person.CompanyVATId,
                Email = person.Email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Phone = person.Phone,
                SignatureUrl = person.SignatureUrl
            };

            var result = await client.PersonPostAsync(dbPerson);
            return result;
        }

        [HttpPatch("person/{id}")]
        public async Task<ActionResult<OpenApiClient.Person>> PatchPerson(string id, [FromBody] Model.NewPerson person)
        {
            httpClient.PassHeaders(Request);
            var dbPerson = await client.PersonGetByIdAsync(id);
            if (dbPerson == null || dbPerson.Data == null)
            {
                return NotFound();
            }
            else
            {
                var operations = await ConvertRecord2Patch(dbPerson.Data, person);                
                var result = await client.PersonPatchAsync(id, operations);
                return result.Data;
            }
        }

        [HttpDelete("person/{id}")]
        public async Task<ActionResult<OpenApiClient.Person>> DeletePerson(string id)
        {
            httpClient.PassHeaders(Request);
            var dbPerson = await client.PersonGetByIdAsync(id);
            if (dbPerson == null || dbPerson.Data == null)
            {
                return NotFound();
            }
            else
            {
                var result = await client.PersonDeleteAsync(id);
                return result.Data;
            }
        }

        [HttpGet("person/{id}")]
        public async Task<ActionResult<OpenApiClient.Person>> GetPerson(string id)
        {
            httpClient.PassHeaders(Request);
            var dbPerson = await client.PersonGetByIdAsync(id);
            if (dbPerson == null || dbPerson.Data == null)
            {
                return NotFound();
            }
            else
            {
                return dbPerson.Data;
            }
        }

        [HttpGet("person")]
        public async Task<ActionResult<IEnumerable<OpenApiClient.PersonDBBase>>> GetPersons([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string query, [FromQuery] string sort)
        {
            httpClient.PassHeaders(Request);
            var addressList = await client.PersonGetAsync(offset, limit, query, sort);
            return addressList.Results.ToList();
        }

        private async Task<List<OpenApiClient.PersonOperation>> ConvertRecord2Patch(OpenApiClient.Person original, Model.NewPerson address)
        {
            try
            {
                var retOperations = new List<OpenApiClient.PersonOperation>(); 
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
                        var op = new OpenApiClient.PersonOperation()
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