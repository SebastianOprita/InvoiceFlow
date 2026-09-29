using InvoiceFlow.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Customers.Infrastructure;

public sealed class CustomersDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public CustomersDbContext(DbContextOptions<CustomersDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
