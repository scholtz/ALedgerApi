using ALedgerApi.Model.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.Mvc;
using ALedgerApi.Model.Comm;

namespace ALedgerApi.Controllers.Data
{
    [Route("")]
    [ApiController]
    public class PersonController : BaseController<
            Person,
            DBBase<Person>,
            DBListBase<Person, DBBase<Person>>,
            DBBaseLog<Person>
        >
    {
        public PersonController(BaseRepository<Person, DBBase<Person>, DBListBase<Person, DBBase<Person>>, DBBaseLog<Person>> repo) : base(repo)
        {
        }
    }
}
