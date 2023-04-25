using Microsoft.AspNetCore.Mvc;

namespace ALedgerApi.Controllers.Data
{
    [Route("v1/[controller]")]
    [ApiController]
    public class BFFController : Controller
    {
        private readonly OpenApiClient.Client client;
        public BFFController() : base()
        {
            var httpClient = new HttpClient();
            client = new OpenApiClient.Client("https://localhost:44375", httpClient);
        }


        [HttpGet("GetTop1")]
        public async Task<OpenApiClient.Person> GetPerson()
        {

            var persons = await client.GetPersonAsync(0, 10, "*");
            return persons.Results.First().Data;
        }
    }
}