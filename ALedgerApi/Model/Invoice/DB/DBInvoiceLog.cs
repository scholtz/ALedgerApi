using ALedgerApi.Model.Person.DB;

namespace ALedgerApi.Model.Invoice.DB
{
    public class DBInvoiceLog : DBInvoice
    {
        public string RefId { get; set; }
        public long Version { get; set; }
    }
}
