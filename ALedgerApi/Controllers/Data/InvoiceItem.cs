using ALedgerApi.Model.Comm;
using ALedgerApi.Model.DB;
using ALedgerApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ALedgerApi.Controllers.Data
{
    [Route("")]
    [ApiController]
    public class InvoiceItemController : BaseController<
            InvoiceItem,
            DBBase<InvoiceItem>,
            DBListBase<InvoiceItem, DBBase<InvoiceItem>>,
            DBBaseLog<InvoiceItem>
        >
    {
        public InvoiceItemController(BaseRepository<InvoiceItem, DBBase<InvoiceItem>, DBListBase<InvoiceItem, DBBase<InvoiceItem>>, DBBaseLog<InvoiceItem>> repo) : base(repo)
        {
        }
    }
}
