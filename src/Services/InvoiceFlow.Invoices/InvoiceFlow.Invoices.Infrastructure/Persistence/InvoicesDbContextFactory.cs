using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class InvoicesDbContextFactory
    : IDesignTimeDbContextFactory<InvoicesDbContext>
{
    public InvoicesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<InvoicesDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=InvoiceFlow_Invoices;Trusted_Connection=True;TrustServerCertificate=True;");

        return new InvoicesDbContext(optionsBuilder.Options);
    }
}
