namespace ALedgerApi.Model.DB
{
    public class DBBase
    {
        public string Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string UpdatedBy { get; set; }
    }
}
