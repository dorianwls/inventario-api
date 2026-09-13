namespace Inventario.Api.Commerce;

public enum PartnerType { Customer, Supplier }

public sealed class Partner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public PartnerType Type { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
