using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ALedgerBFFApi.Controllers
{
    [Authorize]
    [Route("v1/[controller]")]
    [ApiController]
    public class BFFController : Controller
    {
        private readonly OpenApiClient.Client client;
        private readonly HttpClient httpClient = new HttpClient();
        public BFFController() : base()
        {
            client = new OpenApiClient.Client("https://localhost:44375", httpClient);
        }

        [HttpGet("GetTop1")]
        public async Task<OpenApiClient.Person> GetPerson()
        {

            var persons = await client.GetPersonAsync(0, 10, "*");
            return persons.Results.First().Data;
        }
        [HttpPost("IssueInvoice")]
        public async Task<OpenApiClient.Person> IssueInvoice([FromBody] Model.NewInvoice newInvoice)
        {
            var authHeader = Request.Headers.Authorization.ToString()?.Replace("SigTx ","");
            if (!string.IsNullOrEmpty(authHeader))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SigTx", authHeader);
            }
            var issuer = await client.GetPersonByIdAsync(newInvoice.PersonIdIssuer);
            var receiver = await client.GetPersonByIdAsync(newInvoice.PersonIdReceiver);
            var newDBInvoice = new OpenApiClient.Invoice();
            newDBInvoice.NoteBeforeItems = newInvoice.NoteBeforeItems;
            return receiver.Data;
        }
    }
}