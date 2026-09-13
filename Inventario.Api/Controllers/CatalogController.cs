using Inventario.Api.Auth;
using Inventario.Api.Catalog;
using Inventario.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("catalog")]
[Authorize]
public sealed class CatalogController(InventoryDbContext database) : ControllerBase
{
    [HttpGet("categories")]
    public Task<List<Category>> GetCategories() => database.Categories
        .Where(category => category.IsActive)
        .OrderBy(category => category.Name)
        .ToListAsync();

    [HttpPost("categories")]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public async Task<ActionResult<Category>> CreateCategory(CreateCategoryRequest request)
    {
        var category = new Category { Name = request.Name.Trim() };
        database.Categories.Add(category);
        await database.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategories), category);
    }

    [HttpGet("products")]
    public Task<List<ProductSummary>> GetProducts() => database.Products
        .AsNoTracking()
        .Include(product => product.Category)
        .Where(product => product.IsActive)
        .OrderBy(product => product.Name)
        .Select(product => new ProductSummary(product.Id, product.Code, product.Name, product.SuggestedPrice,
            product.MinimumStock, product.CurrentStock, product.Category.Name))
        .ToListAsync();

    [HttpPost("products")]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public async Task<ActionResult<ProductSummary>> CreateProduct(CreateProductRequest request)
    {
        if (request.SuggestedPrice < 0 || request.MinimumStock < 0)
        {
            return BadRequest("Suggested price and minimum stock cannot be negative.");
        }

        if (!await database.Categories.AnyAsync(category => category.Id == request.CategoryId && category.IsActive))
        {
            return BadRequest("The category does not exist or is inactive.");
        }

        var product = new Product
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            SuggestedPrice = request.SuggestedPrice,
            MinimumStock = request.MinimumStock,
            CategoryId = request.CategoryId,
        };
        database.Products.Add(product);
        await database.SaveChangesAsync();
        await database.Entry(product).Reference(item => item.Category).LoadAsync();

        return CreatedAtAction(nameof(GetProducts), new ProductSummary(product.Id, product.Code, product.Name,
            product.SuggestedPrice, product.MinimumStock, product.CurrentStock, product.Category.Name));
    }
}

public sealed record CreateCategoryRequest(string Name);
public sealed record CreateProductRequest(string Code, string Name, decimal SuggestedPrice, int MinimumStock, Guid CategoryId);
public sealed record ProductSummary(Guid Id, string Code, string Name, decimal SuggestedPrice, int MinimumStock, int CurrentStock, string Category);
