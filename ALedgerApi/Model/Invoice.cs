using RestDWH.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("Invoice")]
    public class Invoice
    {
        public bool IsDraft = true;
        public string InvoiceNumber { get; set; }
        public long InvoiceNumberNum { get; set; }
        public string InvoiceType { get; set; }
        public PaymentMethod[] PaymentMethods { get; set; } = Array.Empty<PaymentMethod>();
        public InvoiceSummaryInCurrency[] Summary { get; set; } = Array.Empty<InvoiceSummaryInCurrency>();
        
        public string PersonIdIssuer { get; set; }
        public string PersonIdReceiver { get; set; }
        public DateTimeOffset? DateIssue { get; set; }
        public DateTimeOffset? DateDue { get; set; }
        public DateTimeOffset? DateDelivery { get; set; }
        public string NoteBeforeItems { get; set; }
        public string NoteAfterItems { get; set; }

        public string Currency { get; set; }

    }
}
