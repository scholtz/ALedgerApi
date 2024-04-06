namespace ALedgerBFFApi.Model
{
    public class InvoiceSummaryInCurrency
    {
        public string Currency { get; set; }
        public double Rate { get; set; }
        public string RateCurrencies { get; set; }
        public string RateNote { get; set; }
        public double NetAmount { get; set; }
        public double TaxAmount { get; set; }
        public double GrossAmount { get; set; }
    }
}
