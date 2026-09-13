using System.Security.Claims;
using Inventario.Api.Auth;
using Inventario.Api.Commerce;
using Inventario.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("payments")]
[Authorize]
public sealed class PaymentsController(InventoryDbContext database) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Supervisor},{ApplicationRoles.Accountant}")]
    public async Task<ActionResult> Create(CreatePaymentRequest request)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Note)) return BadRequest();
        var document = await database.CommercialDocuments.SingleOrDefaultAsync(item => item.Id == request.DocumentId);
        if (document is null) return NotFound();
        if (document.PaymentTerms != PaymentTerms.Credit || request.Amount > document.OutstandingBalance) return Conflict("Invalid payment amount.");
        document.OutstandingBalance -= request.Amount;
        database.CommercialPayments.Add(new CommercialPayment { CommercialDocumentId = document.Id, Amount = request.Amount, Note = request.Note.Trim(), CreatedByUserId = Guid.Parse(User.FindFirstValue("sub")!) });
        await database.SaveChangesAsync();
        return NoContent();
    }
}

public sealed record CreatePaymentRequest(Guid DocumentId, decimal Amount, string Note);
