using RestDWH.Attributes;

namespace ALedgerApi.Model.Comm
{
    [RestDWHEntity("TestId")]
    public class TestId
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
