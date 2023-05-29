using RestDWH.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("PaymentMethod")]
    public class PaymentMethod
    {
        /// <summary>
        /// Token id or Currency code
        /// </summary>
        public string Currency { get; set; }
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

    }
}
