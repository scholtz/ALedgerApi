using RestDWH.Base.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("Person", endpointGet: "person", endpointUpsert: "person", endpointPatch: "person", endpointPost: "person", endpointGetById: "person/{id}", endpointDelete: "person")]
    public class Person : IEquatable<Person?>
    {
        public string BusinessName { get; set; }
        public string? CompanyId { get; set; }
        public string? CompanyTaxId { get; set; }
        public string? CompanyVATId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string AddressId { get; set; }
        public string? SignatureUrl { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person? other)
        {
            return other is not null &&
                   BusinessName == other.BusinessName &&
                   CompanyId == other.CompanyId &&
                   CompanyTaxId == other.CompanyTaxId &&
                   CompanyVATId == other.CompanyVATId &&
                   FirstName == other.FirstName &&
                   LastName == other.LastName &&
                   Email == other.Email &&
                   Phone == other.Phone &&
                   AddressId == other.AddressId &&
                   SignatureUrl == other.SignatureUrl;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(BusinessName);
            hash.Add(CompanyId);
            hash.Add(CompanyTaxId);
            hash.Add(CompanyVATId);
            hash.Add(FirstName);
            hash.Add(LastName);
            hash.Add(Email);
            hash.Add(Phone);
            hash.Add(AddressId);
            hash.Add(SignatureUrl);
            return hash.ToHashCode();
        }

        public static bool operator ==(Person? left, Person? right)
        {
            return EqualityComparer<Person>.Default.Equals(left, right);
        }

        public static bool operator !=(Person? left, Person? right)
        {
            return !(left == right);
        }
    }
}
