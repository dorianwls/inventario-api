namespace Inventario.Api.Commerce;

public enum CommercialDocumentType { Purchase, Sale }
public enum PaymentTerms { Cash, Credit }

public sealed class CommercialDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public CommercialDocumentType Type { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public Guid PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;
    public decimal Total { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
