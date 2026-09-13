using Inventario.Api.Auth;
using Inventario.Api.Commerce;
using Inventario.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("partners")]
[Authorize]
public sealed class PartnersController(InventoryDbContext database) : ControllerBase
{
    [HttpGet]
    public Task<List<Partner>> Get([FromQuery] PartnerType? type) => database.Partners.AsNoTracking()
        .Where(partner => partner.IsActive && (type == null || partner.Type == type))
        .OrderBy(partner => partner.Name).ToListAsync();

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.Supervisor}")]
    public async Task<ActionResult<Partner>> Create(CreatePartnerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        var partner = new Partner { Name = request.Name.Trim(), Type = request.Type, Phone = request.Phone?.Trim() };
        database.Partners.Add(partner);
        await database.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), partner);
    }

    [HttpGet("outstanding")]
    public Task<List<object>> Outstanding() => database.CommercialDocuments.AsNoTracking()
        .Include(document => document.Partner).Where(document => document.OutstandingBalance > 0)
        .OrderByDescending(document => document.OutstandingBalance)
        .Select(document => (object)new { document.Id, document.Type, document.Partner.Name, document.Total, document.OutstandingBalance, document.CreatedAt })
        .ToListAsync();
}

public sealed record CreatePartnerRequest(string Name, PartnerType Type, string? Phone);
