using ALedgerApi.Model.Comm;
using ALedgerApi.Model.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ALedgerApi.Controllers.Data
{
    [Route("")]
    [ApiController]
    public class InvoiceController : BaseController<
            Invoice,
            DBBase<Invoice>,
            DBListBase<Invoice, DBBase<Invoice>>,
            DBBaseLog<Invoice>
        >
    {
        public InvoiceController(BaseRepository<Invoice, DBBase<Invoice>, DBListBase<Invoice, DBBase<Invoice>>, DBBaseLog<Invoice>> repo) : base(repo)
        {
        }
    }
}
