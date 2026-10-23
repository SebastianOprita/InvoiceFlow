using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceFlow.Customers.Infrastructure;

public sealed class CustomersDbContextFactory
    : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<CustomersDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=InvoiceFlow_Customers;Trusted_Connection=True;TrustServerCertificate=True;");

        return new CustomersDbContext(optionsBuilder.Options);
    }
}
