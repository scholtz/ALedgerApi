using ALedgerApi.Model.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.Mvc;
using ALedgerApi.Model.Comm;

namespace ALedgerApi.Controllers.Data
{
    [Route("")]
    [ApiController]
    public class TestIdController : BaseController<
            TestId,
            DBBase<TestId>,
            DBListBase<TestId, DBBase<TestId>>,
            DBBaseLog<TestId>
        >
    {
        public TestIdController(BaseRepository<TestId, DBBase<TestId>, DBListBase<TestId, DBBase<TestId>>, DBBaseLog<TestId>> repo) : base(repo)
        {
        }
    }
}
