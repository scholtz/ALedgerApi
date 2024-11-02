namespace ALedgerApi.Model
{
    /// <summary>
    /// Payment details. If invoice has been paid, this structure is filled in. The status indicates if the invoice has been paid.
    /// </summary>
    public class PaymentInfo
    {
        /// <summary>
        /// Status of the payment UNPAID | PAID | PARTIALPAID
        /// </summary>
        public string Status { get; set; } = "UNPAID";

        /// <summary>
        /// single payment detail. One invoice can be paid in multiple txs.
        /// </summary>
        public class PaymentItem
        {
            /// <summary>
            /// Reference of the transaction, usually tx id or bank tx id
            /// </summary>
            public string Reference { get; set; }
            /// <summary>
            /// Network 
            /// 
            /// ALGO | ETH | Bank SWIFT Code
            /// </summary>
            public string Network { get; set; }
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
            /// Amount in the base units
            /// 
            /// Amount in base units, eg on blockchain multiplied with decimals. In fiat multiplied by 2 decimals 10^2
            /// </summary>
            public long BaseAmount { get; set; }

            /// <summary>
            /// Amount in the tx
            /// </summary>
            public decimal? GrossAmount { get; set; }
        }
        /// <summary>
        /// List of real payments
        /// </summary>
        public List<PaymentItem> Payments { get; set; } = [];
    }
}
