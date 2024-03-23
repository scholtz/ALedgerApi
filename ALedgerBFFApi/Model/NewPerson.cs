namespace ALedgerBFFApi.Model
{
    public class NewPerson
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
	}
}
