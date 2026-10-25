using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceFlow.Products.Infrastructure;

public sealed class ProductsDbContextFactory
    : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<ProductsDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=InvoiceFlow_Products;Trusted_Connection=True;TrustServerCertificate=True;");

        return new ProductsDbContext(optionsBuilder.Options);
    }
}
