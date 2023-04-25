namespace ALedgerBFFApi.Model
{
    public class NewInvoice
    {
        public string InvoiceType { get; set; }
        public string PersonIdIssuer { get; set; }
        public string PersonIdReceiver { get; set; }
        public InvoiceItem[] Items { get; set; }
        public bool IsDraft { get; set; } = true;
        public int PayableInDays { get; set; } = 14;

        public DateTimeOffset? DateDelivery { get; set; }
        public string NoteBeforeItems { get; set; }
        public string NoteAfterItems { get; set; }
    }
}
