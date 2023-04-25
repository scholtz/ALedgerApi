using ALedgerApi.Model.Comm;
using ALedgerApi.Model.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ALedgerApi.Controllers.Data
{
    [Route("")]
    [ApiController]
    public class AddressController : BaseController<
            Address,
            DBBase<Address>,
            DBListBase<Address, DBBase<Address>>,
            DBBaseLog<Address>
        >
    {
        public AddressController(BaseRepository<Address, DBBase<Address>, DBListBase<Address, DBBase<Address>>, DBBaseLog<Address>> repo) : base(repo)
        {
        }
    }
}
