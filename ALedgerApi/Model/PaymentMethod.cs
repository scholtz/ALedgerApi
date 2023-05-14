using RestDWH.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("PaymentMethod")]
    public class PaymentMethod : IEquatable<PaymentMethod?>
    {
        public string Type { get; set; }
        public string Account { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PaymentMethod);
        }

        public bool Equals(PaymentMethod? other)
        {
            return other is not null &&
                   Type == other.Type &&
                   Account == other.Account;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Account);
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
