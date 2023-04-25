using RestDWH.Attributes;

namespace ALedgerApi.Model.Comm
{
    /// <summary>
    /// Address object
    /// </summary>
    [RestDWHEntity("Address")]
    public class Address : IEquatable<Address?>
    {
        /// <summary>
        /// Example: Dopravaku 5
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Second line of street
        /// </summary>
        public string? StreetLine2 { get; set; }
        /// <summary>
        /// City
        /// 
        /// Example: Prague
        /// </summary>
        public string City { get; set; }
        public string? State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Address);
        }

        public bool Equals(Address? other)
        {
            return other is not null &&
                   Street == other.Street &&
                   StreetLine2 == other.StreetLine2 &&
                   City == other.City &&
                   State == other.State &&
                   ZipCode == other.ZipCode &&
                   Country == other.Country &&
                   CountryCode == other.CountryCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, StreetLine2, City, State, ZipCode, Country, CountryCode);
        }

        public static bool operator ==(Address? left, Address? right)
        {
            return EqualityComparer<Address>.Default.Equals(left, right);
        }

        public static bool operator !=(Address? left, Address? right)
        {
            return !(left == right);
        }
    }
}
