using ALedgerApi.Model.Invoice.DB;

namespace ALedgerApi.Model.InvoiceItem.DB
{
    public class DBInvoiceItemList : Model.DB.DBListBase
    {
        public DBInvoiceItem[] Results { get; set; }
    }
}
