namespace ALedgerApi.Model.Comm
{
    public class InvoiceItem
    {
        public string InvoiceId { get; set; }
        public string ItemText { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal GrossAmount { get; set; }
    }
}
