using ALedgerApi.Events;
using RestDWH.Base.Attributes;
using RestDWH.Elastic.Attributes.Endpoints;
using RestDWHBase.Attributes.Endpoints;

namespace ALedgerApi.Model
{
    [RestDWHEndpointGet]
    [RestDWHEndpointGetById]
    [RestDWHEndpointUpsert]
    [RestDWHEndpointPatch]
    [RestDWHEndpointDelete]
    [RestDWHEndpointProperties]
    [RestDWHEndpointElasticQuery]
    [RestDWHEndpointElasticPropertiesQuery]
    [RestDWHEndpointPost]
    [RestDWHEntity("InvoiceItem", typeof(InvoiceItemEvents), apiName: "invoice-item")]
    public class InvoiceItem : IEquatable<InvoiceItem?>
    {
        public string ItemText { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; }
        public decimal Discount { get; set; }
        public decimal Quantity { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal NetAmount { get; set; }
        public decimal GrossAmount { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as InvoiceItem);
        }

        public bool Equals(InvoiceItem? other)
        {
            return other is not null &&
                   ItemText == other.ItemText &&
                   UnitPrice == other.UnitPrice &&
                   Discount == other.Discount &&
                   Quantity == other.Quantity &&
                   TaxPercent == other.TaxPercent &&
                   NetAmount == other.NetAmount &&
                   GrossAmount == other.GrossAmount;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemText, UnitPrice, Discount, Quantity, TaxPercent, NetAmount, GrossAmount);
        }

        public static bool operator ==(InvoiceItem? left, InvoiceItem? right)
        {
            return EqualityComparer<InvoiceItem>.Default.Equals(left, right);
        }

        public static bool operator !=(InvoiceItem? left, InvoiceItem? right)
        {
            return !(left == right);
        }
    }
}
