namespace ALedgerApi.Model.InvoiceItem.Comm
{
    public class InvoiceItem
    {
        public string ItemText { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal GrossAmount { get; set; }
    }
}
