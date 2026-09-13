using System.Data;
using System.Security.Claims;
using Inventario.Api.Auth;
using Inventario.Api.Catalog;
using Inventario.Api.Commerce;
using Inventario.Api.Inventory;
using Inventario.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("commerce")]
[Authorize]
public sealed class CommerceController(InventoryDbContext database) : ControllerBase
{
    [HttpPost("purchases")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Supervisor}")]
    public Task<ActionResult> Purchase(CreateCommerceRequest request) => Create(request, CommercialDocumentType.Purchase);

    [HttpPost("sales")]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Supervisor},{ApplicationRoles.Seller}")]
    public Task<ActionResult> Sale(CreateCommerceRequest request) => Create(request, CommercialDocumentType.Sale);

    private async Task<ActionResult> Create(CreateCommerceRequest request, CommercialDocumentType type)
    {
        if (request.Quantity <= 0 || request.UnitPrice < 0 || string.IsNullOrWhiteSpace(request.Reason)) return BadRequest();
        var expectedPartner = type == CommercialDocumentType.Purchase ? PartnerType.Supplier : PartnerType.Customer;
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var product = await database.Products.SingleOrDefaultAsync(item => item.Id == request.ProductId && item.IsActive);
        var partner = await database.Partners.SingleOrDefaultAsync(item => item.Id == request.PartnerId && item.IsActive && item.Type == expectedPartner);
        if (product is null || partner is null) return BadRequest("Product or partner is invalid.");
        if (type == CommercialDocumentType.Sale && product.CurrentStock < request.Quantity) return Conflict("Insufficient stock.");

        var delta = type == CommercialDocumentType.Purchase ? request.Quantity : -request.Quantity;
        if (type == CommercialDocumentType.Purchase)
            product.AverageCost = ((product.CurrentStock * product.AverageCost) + (request.Quantity * request.UnitPrice)) / (product.CurrentStock + request.Quantity);
        product.CurrentStock += delta;
        var total = request.Quantity * request.UnitPrice;
        database.CommercialDocuments.Add(new CommercialDocument { Type = type, PaymentTerms = request.PaymentTerms, PartnerId = partner.Id, Total = total, OutstandingBalance = request.PaymentTerms == PaymentTerms.Credit ? total : 0 });
        database.InventoryMovements.Add(new InventoryMovement { ProductId = product.Id, Type = type == CommercialDocumentType.Purchase ? InventoryMovementType.Entry : InventoryMovementType.Exit, Quantity = delta, UnitCost = type == CommercialDocumentType.Purchase ? request.UnitPrice : product.AverageCost, ResultingStock = product.CurrentStock, Reason = request.Reason.Trim(), CreatedByUserId = Guid.Parse(User.FindFirstValue("sub")!) });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return NoContent();
    }
}

public sealed record CreateCommerceRequest(Guid PartnerId, Guid ProductId, int Quantity, decimal UnitPrice, PaymentTerms PaymentTerms, string Reason);
