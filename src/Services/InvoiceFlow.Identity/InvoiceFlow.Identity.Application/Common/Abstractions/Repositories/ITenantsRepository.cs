using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface ITenantsRepository
{
    public Task<bool> ExistsBySlugAsync(TenantSlug slug);
    public Task<List<Tenant>> FindAllTenantsAsync();
    public Task<Tenant?> FindTenantByIdAsync(Guid id);
    public Task<Tenant?> FindTenantBySlugAsync(string slug);
    public Task<Tenant?> GetTenantByIdAsync(Guid id);
    public void AddTenant(Tenant tenant);
}
