using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Identity.Infrastructure;

public sealed class ServiceClientTenantConfiguration
    : IEntityTypeConfiguration<ServiceClientTenant>
{
    public void Configure(EntityTypeBuilder<ServiceClientTenant> builder)
    {
        builder.ToTable("ServiceClientTenants");

        builder.HasKey(x => new
        {
            x.ServiceClientId,
            x.TenantId
        });

        builder.Property(x => x.ServiceClientId).IsRequired();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_ServiceClientTenants_TenantId");
    }
}
