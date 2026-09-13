namespace Inventario.Api.Catalog;

public sealed class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SuggestedPrice { get; set; }
    public int MinimumStock { get; set; }
    public int CurrentStock { get; set; }
    public decimal AverageCost { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
