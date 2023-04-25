using RestDWH.Attributes;

namespace ALedgerApi.Model.Comm
{
    [RestDWHEntity("PaymentMethod")]
    public class PaymentMethod
    {
        public string Type { get; set; }
        public string Account { get; set; }
    }
}
