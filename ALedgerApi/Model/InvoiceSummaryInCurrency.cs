namespace ALedgerApi.Model
{
    public class InvoiceSummaryInCurrency
    {
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public string RateNote { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
    }
}
