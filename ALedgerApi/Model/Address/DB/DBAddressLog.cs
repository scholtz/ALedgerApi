using ALedgerApi.Model.Person.DB;

namespace ALedgerApi.Model.Address.DB
{
    public class DBAddressLog : DBAddress
    {
        public string RefId { get; set; }
        public long Version { get; set; }
    }
}
