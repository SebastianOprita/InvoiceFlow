using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Payments.Infrastructure;

public sealed class PaymentsDbContext : DbContext
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<PaymentNumberSequence> PaymentNumberSequences => Set<PaymentNumberSequence>();
    public DbSet<InvoicePaymentBalance> InvoicePaymentBalances => Set<InvoicePaymentBalance>();

    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
