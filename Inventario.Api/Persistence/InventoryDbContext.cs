using Inventario.Api.Auth;
using Inventario.Api.Catalog;
using Inventario.Api.Inventory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("inventory");
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(150);
        });
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(category => category.Name).IsUnique();
        });
        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Code).HasMaxLength(50).IsRequired();
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.SuggestedPrice).HasPrecision(18, 2);
            entity.Property(product => product.AverageCost).HasPrecision(18, 4);
            entity.HasIndex(product => product.Code).IsUnique();
            entity.HasOne(product => product.Category).WithMany().HasForeignKey(product => product.CategoryId);
        });
        builder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("inventory_movements");
            entity.HasKey(movement => movement.Id);
            entity.Property(movement => movement.UnitCost).HasPrecision(18, 4);
            entity.Property(movement => movement.Reason).HasMaxLength(500).IsRequired();
            entity.HasIndex(movement => new { movement.ProductId, movement.CreatedAt });
        });
    }
}
