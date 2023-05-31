using RestDWH.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("PaymentMethod")]
    public class PaymentMethod : IEquatable<PaymentMethod?>
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
        public decimal GrossAmount { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PaymentMethod);
        }

        public bool Equals(PaymentMethod? other)
        {
            return other is not null &&
                   Currency == other.Currency &&
                   CurrencyId == other.CurrencyId &&
                   Account == other.Account &&
                   Network == other.Network;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Currency, CurrencyId, Account, Network);
        }

        public static bool operator ==(PaymentMethod? left, PaymentMethod? right)
        {
            return EqualityComparer<PaymentMethod>.Default.Equals(left, right);
        }

        public static bool operator !=(PaymentMethod? left, PaymentMethod? right)
        {
            return !(left == right);
        }
    }
}
