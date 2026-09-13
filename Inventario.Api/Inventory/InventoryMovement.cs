using Inventario.Api.Catalog;

namespace Inventario.Api.Inventory;

public enum InventoryMovementType { Entry, Exit, Adjustment }

public sealed class InventoryMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public InventoryMovementType Type { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public int ResultingStock { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
