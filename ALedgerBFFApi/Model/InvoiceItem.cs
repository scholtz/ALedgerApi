namespace ALedgerBFFApi.Model
{
    public class InvoiceItem
    {
        public string ItemText { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; } = 0;
        public string Currency { get; set; }
        public decimal Quantity { get; set; } = 0;
        public decimal TaxPercent { get; set; } = 0;
    }
}
