namespace ALedgerBFFApi.Model
{
    public class InvoiceItem
    {
        public string ItemText { get; set; } = string.Empty;
        public double UnitPrice { get; set; } = 0;
        public string Currency { get; set; }
        public double Quantity { get; set; } = 0;
        public double TaxPercent { get; set; } = 0;
        public double Discount { get; set; }
        public double GrossAmount { get; set; }
        public double NetAmount { get; set; }
        public string Unit { get; set; }
    }
}
