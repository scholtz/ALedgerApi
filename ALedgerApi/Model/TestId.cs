using ALedgerApi.Events;
using RestDWH.Attributes;

namespace ALedgerApi.Model
{
    [RestDWHEntity("Test")]
    public class Test
    {
        public string Name { get; set; }
    }
}
