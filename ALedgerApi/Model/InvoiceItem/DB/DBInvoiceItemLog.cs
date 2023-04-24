using ALedgerApi.Model.Invoice.DB;

namespace ALedgerApi.Model.InvoiceItem.DB
{
    public class DBInvoiceItemLog : DBInvoiceItem
    {
        public string RefId { get; set; }
        public long Version { get; set; }
    }
}
