using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceFlow.Payments.Infrastructure;

public sealed class PaymentsDbContextFactory
    : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<PaymentsDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=InvoiceFlow_Payments;Trusted_Connection=True;TrustServerCertificate=True;");

        return new PaymentsDbContext(optionsBuilder.Options);
    }
}
