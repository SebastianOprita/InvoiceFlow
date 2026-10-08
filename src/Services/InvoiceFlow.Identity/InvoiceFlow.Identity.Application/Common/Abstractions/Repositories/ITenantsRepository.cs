using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface ITenantsRepository
{
    public Task<bool> ExistsBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default);
    public Task<List<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    public Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Tenant?> GetTenantBySlugAsync(string slug, CancellationToken cancellationToken = default);
    public Task<Tenant?> GetTrackedTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public void AddTenant(Tenant tenant);
}
