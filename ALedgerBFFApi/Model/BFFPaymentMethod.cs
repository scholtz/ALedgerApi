namespace ALedgerBFFApi.Model
{
    public class BFFPaymentMethod
    {
        /// <summary>
        /// Token name or Currency code
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Token id or Currency code
        /// </summary>
        public string CurrencyId { get; set; }
        /// <summary>
        /// Account number.
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// Network 
        /// 
        /// ALGO | ETH | Bank SWIFT Code
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// Amount on invoice to be paid
        /// </summary>
        public double? GrossAmount { get; set; }
    }
}
