namespace ALedgerApi.Model.DB
{
    public class DBBase<TComm>
    {
        public string Id { get; set; }
        public TComm? Data { get; set; } = default;
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
