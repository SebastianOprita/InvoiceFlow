using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public static class TenantMapper
{
    public static TenantDto ToDto(this Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name.Value,
            Slug = tenant.Slug.Value,
            IsActive = tenant.IsActive,
            CreatedAtUtc = tenant.CreatedAtUtc,
            UpdatedAtUtc = tenant.UpdatedAtUtc
        };
    }
}
