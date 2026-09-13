namespace Inventario.Api.Commerce;

public sealed class CommercialPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommercialDocumentId { get; set; }
    public CommercialDocument CommercialDocument { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
