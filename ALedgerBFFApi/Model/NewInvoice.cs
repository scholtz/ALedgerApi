namespace ALedgerBFFApi.Model
{
    public class NewInvoice
    {
        public string InvoiceType { get; set; }
        public string PersonIdIssuer { get; set; }
        public string PersonIdReceiver { get; set; }
        public BFFInvoiceItem[] Items { get; set; }
        public bool IsDraft { get; set; } = true;
        public int PayableInDays { get; set; } = 14;

        public DateTimeOffset? DateDelivery { get; set; }
        public string NoteBeforeItems { get; set; }
        public string NoteAfterItems { get; set; }

        public string Currency { get; set; }
        public DateTimeOffset? DateDue { get; set; }
        public DateTimeOffset? DateIssue { get; set; }
        public string InvoiceNumber { get; set; }
        public long InvoiceNumberNum { get; set; }
        public BFFPaymentMethod[] PaymentMethods { get; set; }
        public BFFInvoiceSummaryInCurrency[] Summary { get; set; }
        public OpenApiClient.Invoice ToOpenApi()
        {
            return new OpenApiClient.Invoice()
            {
                InvoiceType = InvoiceType,
                Currency = Currency,
                DateDelivery = DateDelivery,
                DateIssue = DateIssue,
                DateDue = DateDue,
                InvoiceNumber = InvoiceNumber,
                InvoiceNumberNum = InvoiceNumberNum,
                NoteAfterItems = NoteAfterItems,
                NoteBeforeItems = NoteBeforeItems,
                PersonIdIssuer = PersonIdIssuer,
                PersonIdReceiver = PersonIdReceiver,
                Items = Items?.Select(i => i.ToOpenApi()).ToArray() ?? [] ,
                PaymentMethods = PaymentMethods?.Select(i => i.ToOpenApi()).ToArray() ?? [],
                Summary = Summary.Select(i => i.ToOpenApi()).ToArray() ?? []
            };
        }
    }
}
