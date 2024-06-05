using ALedgerApi.Events;
using RestDWH.Base.Attributes;
using RestDWH.Elastic.Attributes.Endpoints;
using RestDWHBase.Attributes.Endpoints;

namespace ALedgerApi.Model
{
    [RestDWHEndpointGet]
    [RestDWHEndpointGetById]
    [RestDWHEndpointPut]
    //[RestDWHEndpointUpsert]
    [RestDWHEndpointPatch]
    [RestDWHEndpointDelete]
    [RestDWHEndpointProperties]
    [RestDWHEndpointElasticQuery]
    [RestDWHEndpointElasticPropertiesQuery]
    [RestDWHEndpointPost]
    [RestDWHEntity("Person", typeof(PersonEvents), apiName: "person")]
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
        public string? AddressId { get; set; }
        public Address Address { get; set; } = new Address();
        public string? SignatureUrl { get; set; }

        public bool Equals(Person? obj)
        {
            return obj is Person person &&
                   BusinessName == person.BusinessName &&
                   CompanyId == person.CompanyId &&
                   CompanyTaxId == person.CompanyTaxId &&
                   CompanyVATId == person.CompanyVATId &&
                   FirstName == person.FirstName &&
                   LastName == person.LastName &&
                   Email == person.Email &&
                   Phone == person.Phone &&
                   AddressId == person.AddressId &&
                   EqualityComparer<Address>.Default.Equals(Address, person.Address) &&
                   SignatureUrl == person.SignatureUrl;
        }

        public override bool Equals(object? obj)
        {
            return obj is Person person &&
                   BusinessName == person.BusinessName &&
                   CompanyId == person.CompanyId &&
                   CompanyTaxId == person.CompanyTaxId &&
                   CompanyVATId == person.CompanyVATId &&
                   FirstName == person.FirstName &&
                   LastName == person.LastName &&
                   Email == person.Email &&
                   Phone == person.Phone &&
                   AddressId == person.AddressId &&
                   EqualityComparer<Address>.Default.Equals(Address, person.Address) &&
                   SignatureUrl == person.SignatureUrl;
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
            hash.Add(Address);
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
