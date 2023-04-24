namespace ALedgerApi.Model.Invoice.Comm
{
    public class Invoice
    {
        public string InvoiceNumber { get; set; }
        public long InvoiceNumberNum { get; set; }
        public string InvoiceType { get; set; }
        public string PersonIdIssuer { get; set; }
        public string PersonIdReceiver { get; set; }
        public bool IsDraft = true;
        public DateTimeOffset? DateIssue { get; set; }
        public DateTimeOffset? DateDue { get; set; }
        public DateTimeOffset? DateDelivery { get; set; }
        public string NoteBeforeItems { get; set; }
        public string NoteAfterItems { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrossAmount { get; set; }
    }
}
