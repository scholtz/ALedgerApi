using ALedgerApi.Model.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Swashbuckle.AspNetCore.Annotations;

namespace ALedgerApi.Controllers
{
    public class BaseController<TEnt, TDBEnt, TDBEntList, TDBEntLog> : ControllerBase
        where TEnt : class
        where TDBEnt : DBBase<TEnt>
        where TDBEntList : DBListBase<TEnt, TDBEnt>
        where TDBEntLog : DBBaseLog<TEnt>
    {
        private readonly BaseRepository<
            TEnt,
            TDBEnt,
            TDBEntList,
            TDBEntLog
        > repo;
        public BaseController(BaseRepository<
            TEnt,
            TDBEnt,
            TDBEntList,
            TDBEntLog
        > repo)
        {
            this.repo = repo;
        }

        public static string Name<T>()
        {
            return typeof(T).FullName;
        }

        [HttpGet($"v1/Get[controller]")]
        public Task<TDBEntList> Get(int from = 0, int size = 10, string query = "*")
        {
            return repo.Get(from, size, query);
        }

        [HttpGet("v1/Get[controller]ById/{id}")]
        public Task<TDBEnt?> GetById(string id)
        {
            return repo.GetById(id);
        }

        [SwaggerOperation("Post")]
        [HttpPost("v1/Post[controller]")]
        public Task<TDBEnt> Post([FromBody] TEnt data)
        {
            return repo.Post(data);
        }

        [SwaggerOperation("Put")]
        [HttpPut("v1/Put[controller]/{id}")]
        public Task<TDBEnt> Put([FromRoute] string id, [FromBody] TEnt data)
        {
            return repo.Put(id, data);
        }


        [SwaggerOperation("Put")]
        [HttpPut("v1/Upsert[controller]/{id}")]
        public Task<TDBEnt> Upsert([FromRoute] string id, [FromBody] TEnt data)
        {
            return repo.Upsert(id, data);
        }

        [SwaggerOperation("Patch")]
        [HttpPatch("v1/Patch[controller]/{id}")]
        public Task<TDBEnt> Patch([FromRoute] string id, [FromBody] JsonPatchDocument<TEnt> data)
        {
            return repo.Patch(id, data);
        }

        [HttpDelete("v1/Delete[controller]/{id}")]
        public Task<TDBEnt> Delete(string id)
        {
            return repo.Delete(id);
        }
    }
}
