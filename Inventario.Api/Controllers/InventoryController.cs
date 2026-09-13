using System.Data;
using System.Security.Claims;
using Inventario.Api.Auth;
using Inventario.Api.Inventory;
using Inventario.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("inventory")]
[Authorize]
public sealed class InventoryController(InventoryDbContext database) : ControllerBase
{
    [HttpPost("movements")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Supervisor},{ApplicationRoles.Warehouse}")]
    public async Task<ActionResult> CreateMovement(CreateMovementRequest request)
    {
        if (request.Quantity == 0 || string.IsNullOrWhiteSpace(request.Reason) || request.UnitCost < 0)
            return BadRequest("Quantity, unit cost, and reason are required.");

        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var product = await database.Products.SingleOrDefaultAsync(item => item.Id == request.ProductId && item.IsActive);
        if (product is null) return NotFound();

        var delta = request.Type == InventoryMovementType.Exit ? -Math.Abs(request.Quantity) : request.Quantity;
        var resultingStock = product.CurrentStock + delta;
        if (resultingStock < 0) return Conflict("Insufficient stock.");

        if (request.Type == InventoryMovementType.Entry && delta > 0)
        {
            product.AverageCost = ((product.CurrentStock * product.AverageCost) + (delta * request.UnitCost)) / resultingStock;
        }

        product.CurrentStock = resultingStock;
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        database.InventoryMovements.Add(new InventoryMovement { ProductId = product.Id, Type = request.Type,
            Quantity = delta, UnitCost = request.UnitCost, ResultingStock = resultingStock,
            Reason = request.Reason.Trim(), CreatedByUserId = userId });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return NoContent();
    }

    [HttpGet("low-stock")]
    public Task<List<object>> GetLowStock() => database.Products.AsNoTracking()
        .Where(product => product.IsActive && product.CurrentStock <= product.MinimumStock)
        .OrderBy(product => product.CurrentStock)
        .Select(product => (object)new { product.Id, product.Code, product.Name, product.CurrentStock, product.MinimumStock })
        .ToListAsync();

    [HttpGet("products/{productId:guid}/kardex")]
    public Task<List<InventoryMovement>> GetKardex(Guid productId) => database.InventoryMovements.AsNoTracking()
        .Where(movement => movement.ProductId == productId).OrderByDescending(movement => movement.CreatedAt).ToListAsync();
}

public sealed record CreateMovementRequest(Guid ProductId, InventoryMovementType Type, int Quantity, decimal UnitCost, string Reason);
