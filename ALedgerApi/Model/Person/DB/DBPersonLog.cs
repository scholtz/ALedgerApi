namespace ALedgerApi.Model.Person.DB
{
    public class DBPersonLog : DBPerson
    {
        public string RefId { get; set; }
        public long Version { get; set; }
    }
}
