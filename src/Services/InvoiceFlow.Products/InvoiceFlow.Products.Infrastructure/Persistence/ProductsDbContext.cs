using InvoiceFlow.Products.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Products.Infrastructure;

public sealed class ProductsDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
