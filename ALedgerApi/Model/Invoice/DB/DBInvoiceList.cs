using ALedgerApi.Model.Address.DB;

namespace ALedgerApi.Model.Invoice.DB
{
    public class DBInvoiceList : Model.DB.DBListBase
    {
        public DBInvoice[] Results { get; set; }
    }
}
