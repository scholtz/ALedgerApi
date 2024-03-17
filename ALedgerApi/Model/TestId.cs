using ALedgerApi.Events;
using RestDWH.Base.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("Test")]
    public class Test
    {
        public string Name { get; set; }
    }
}
