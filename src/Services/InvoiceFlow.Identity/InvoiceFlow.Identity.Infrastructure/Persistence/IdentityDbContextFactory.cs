using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class IdentityDbContextFactory
    : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<IdentityDbContext>();

        optionsBuilder.UseSqlServer("Server=localhost;Database=InvoiceFlow_Identities;Trusted_Connection=True;TrustServerCertificate=True;");

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
