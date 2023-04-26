using RestDWH.Attributes;

namespace ALedgerApi.Model.Comm
{
    [RestDWHEntity("Invoice")]
    public class Invoice : IEquatable<Invoice?>
    {
        public string InvoiceNumber { get; set; }
        public long InvoiceNumberNum { get; set; }
        public string InvoiceType { get; set; }
        public string[] PaymentMethodIds { get; set; } = Array.Empty<string>();
        public string PersonIdIssuer { get; set; }
        public string PersonIdReceiver { get; set; }
        public bool IsDraft = true;
        public DateTimeOffset? DateIssue { get; set; }
        public DateTimeOffset? DateDue { get; set; }
        public DateTimeOffset? DateDelivery { get; set; }
        public string NoteBeforeItems { get; set; }
        public string NoteAfterItems { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrossAmount { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Invoice);
        }

        public bool Equals(Invoice? other)
        {
            return other is not null &&
                   InvoiceNumber == other.InvoiceNumber &&
                   InvoiceNumberNum == other.InvoiceNumberNum &&
                   InvoiceType == other.InvoiceType &&
                   EqualityComparer<string[]>.Default.Equals(PaymentMethodIds, other.PaymentMethodIds) &&
                   PersonIdIssuer == other.PersonIdIssuer &&
                   PersonIdReceiver == other.PersonIdReceiver &&
                   IsDraft == other.IsDraft &&
                   EqualityComparer<DateTimeOffset?>.Default.Equals(DateIssue, other.DateIssue) &&
                   EqualityComparer<DateTimeOffset?>.Default.Equals(DateDue, other.DateDue) &&
                   EqualityComparer<DateTimeOffset?>.Default.Equals(DateDelivery, other.DateDelivery) &&
                   NoteBeforeItems == other.NoteBeforeItems &&
                   NoteAfterItems == other.NoteAfterItems &&
                   NetAmount == other.NetAmount &&
                   TotalTax == other.TotalTax &&
                   GrossAmount == other.GrossAmount;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(InvoiceNumber);
            hash.Add(InvoiceNumberNum);
            hash.Add(InvoiceType);
            hash.Add(PaymentMethodIds);
            hash.Add(PersonIdIssuer);
            hash.Add(PersonIdReceiver);
            hash.Add(IsDraft);
            hash.Add(DateIssue);
            hash.Add(DateDue);
            hash.Add(DateDelivery);
            hash.Add(NoteBeforeItems);
            hash.Add(NoteAfterItems);
            hash.Add(NetAmount);
            hash.Add(TotalTax);
            hash.Add(GrossAmount);
            return hash.ToHashCode();
        }

        public static bool operator ==(Invoice? left, Invoice? right)
        {
            return EqualityComparer<Invoice>.Default.Equals(left, right);
        }

        public static bool operator !=(Invoice? left, Invoice? right)
        {
            return !(left == right);
        }
    }
}
