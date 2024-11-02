using ALedgerApi.Events;
using RestDWH.Base.Attributes;
using RestDWH.Elastic.Attributes.Endpoints;
using RestDWHBase.Attributes.Endpoints;

namespace ALedgerApi.Model
{    
    /// <summary>
    /// Invoice object
    /// </summary>
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
        /// <summary>
        /// Indicates if payment is draft or has been sent to third party
        /// </summary>
        public bool IsDraft = true;
        /// <summary>
        /// Invoice number in text format
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// Invoice number in number format which is incremented by one each new invoice.
        /// </summary>
        public long InvoiceNumberNum { get; set; }
        /// <summary>
        /// issued | received
        /// </summary>
        public string InvoiceType { get; set; }
        /// <summary>
        /// Payment methods
        /// </summary>
        public PaymentMethod[] PaymentMethods { get; set; } = Array.Empty<PaymentMethod>();
        /// <summary>
        /// Summary of the invoice
        /// </summary>
        public InvoiceSummaryInCurrency[] Summary { get; set; } = Array.Empty<InvoiceSummaryInCurrency>();
        /// <summary>
        /// Items on the invoice
        /// </summary>
        public InvoiceItem[] Items { get; set; } = Array.Empty<InvoiceItem>();
        /// <summary>
        /// Referece id of the person who issued the invoice
        /// </summary>
        public string PersonIdIssuer { get; set; }
        /// <summary>
        /// Referece id of the person who is receiver
        /// </summary>
        public string PersonIdReceiver { get; set; }
        /// <summary>
        /// Date of the issue
        /// </summary>
        public DateTimeOffset? DateIssue { get; set; }
        /// <summary>
        /// Date when invoice should be paid
        /// </summary>
        public DateTimeOffset? DateDue { get; set; }
        /// <summary>
        /// Date of the delivery of the product or service
        /// </summary>
        public DateTimeOffset? DateDelivery { get; set; }
        /// <summary>
        /// Note before the invoice items
        /// </summary>
        public string NoteBeforeItems { get; set; }
        /// <summary>
        /// Note after the invoice items
        /// </summary>
        public string NoteAfterItems { get; set; }
        /// <summary>
        /// Main currency of the invoice
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Payment details. PaymentInfo.Status indicates if invoice has been paid.
        /// </summary>
        public PaymentInfo PaymentInfo { get; set; } = new PaymentInfo();
    }
}
