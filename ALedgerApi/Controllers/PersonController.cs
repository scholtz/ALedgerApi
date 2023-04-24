using ALedgerApi.Model.Person.Comm;
using ALedgerApi.Model.Person.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace ALedgerApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly PersonRepository personRepository;
        public PersonController(PersonRepository personRepository)
        {
            this.personRepository = personRepository;
        }

        [HttpGet]
        public Task<DBPersonList> Get(int from = 0, int size = 10, string query = "*")
        {
            return personRepository.Get(from, size, query);
        }

        [HttpGet("{id}")]
        public Task<DBPerson?> GetPerson(string id)
        {
            return personRepository.GetPerson(id);
        }

        [HttpPost]
        public Task<DBPerson> Post([FromBody] Model.Person.Comm.Person person)
        {
            return personRepository.Post(person);
        }

        [HttpPut("{id}")]
        public Task<DBPerson> Put([FromRoute] string id, [FromBody] Model.Person.Comm.Person person)
        {
            return personRepository.Put(id, person);
        }

        [HttpPatch("{id}")]
        public Task<DBPerson> Patch([FromRoute] string id, [FromBody] JsonPatchDocument<Model.Person.Comm.Person> person)
        {
            return personRepository.Patch(id, person);
        }

        [HttpDelete("{id}")]
        public Task<bool> Delete(string id)
        {
            return personRepository.Delete(id);
        }
    }
}
