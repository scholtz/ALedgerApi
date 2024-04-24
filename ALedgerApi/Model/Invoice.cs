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
    [RestDWHEntity("Invoice", typeof(InvoiceEvents), apiName: "invoice")]
    public class Invoice
    {
        public bool IsDraft = true;
        public string InvoiceNumber { get; set; }
        public long InvoiceNumberNum { get; set; }
        /// <summary>
        /// issued | received
        /// </summary>
        public string InvoiceType { get; set; }
        public PaymentMethod[] PaymentMethods { get; set; } = Array.Empty<PaymentMethod>();
        public InvoiceSummaryInCurrency[] Summary { get; set; } = Array.Empty<InvoiceSummaryInCurrency>();
        public InvoiceItem[] Items { get; set; } = Array.Empty<InvoiceItem>();
        public string PersonIdIssuer { get; set; }
        public string PersonIdReceiver { get; set; }
        public DateTimeOffset? DateIssue { get; set; }
        public DateTimeOffset? DateDue { get; set; }
        public DateTimeOffset? DateDelivery { get; set; }
        public string NoteBeforeItems { get; set; }
        public string NoteAfterItems { get; set; }

        public string Currency { get; set; }

    }
}
