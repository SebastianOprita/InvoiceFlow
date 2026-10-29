using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Operation)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Key)
            .HasConversion(
                key => key.Value,
                value => IdempotencyKey.Create(value))
            .HasMaxLength(IdempotencyKey.MaxLength)
            .HasColumnName("Key")
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasConversion(
                hash => hash.Value,
                value => RequestHash.Create(value))
            .HasMaxLength(RequestHash.Sha256HexLength)
            .IsUnicode(false)
            .HasColumnName("RequestHash")
            .IsRequired();

        builder.Property(x => x.ResourceId)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Operation,
            x.Key
        })
        .IsUnique()
        .HasDatabaseName("UX_IdempotencyRecords_TenantId_Operation_Key");
    }
}
