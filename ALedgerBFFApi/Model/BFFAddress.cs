namespace ALedgerBFFApi.Model
{
    public class BFFAddress
    {
        public string Street { get; set; }
        public string? StreetLine2 { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
    }
}
