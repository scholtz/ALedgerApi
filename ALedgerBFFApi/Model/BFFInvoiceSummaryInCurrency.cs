namespace ALedgerBFFApi.Model
{
    public class BFFInvoiceSummaryInCurrency
    {
        public string Currency { get; set; }
        public double Rate { get; set; }
        public string RateCurrencies { get; set; }
        public string RateNote { get; set; }
        public double NetAmount { get; set; }
        public double TaxAmount { get; set; }
        public double GrossAmount { get; set; }
        /// <summary>
        /// Convert to openapi spec
        /// </summary>
        /// <returns></returns>
        public OpenApiClient.InvoiceSummaryInCurrency ToOpenApi()
        {
            return new OpenApiClient.InvoiceSummaryInCurrency()
            {
                Currency = Currency,
                Rate = Rate,
                RateCurrencies = RateCurrencies,
                RateNote = RateNote,
                NetAmount = NetAmount,
                TaxAmount = TaxAmount,
                GrossAmount = GrossAmount
            };
        }
    }
}
