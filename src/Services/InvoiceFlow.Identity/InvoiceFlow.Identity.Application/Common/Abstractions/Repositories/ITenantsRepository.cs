using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface ITenantsRepository
{
    public bool ExistsBySlug(TenantSlug slug);
    public List<Tenant> FindAllTenants();
    public Tenant? FindTenantById(Guid id);
    public Tenant? FindTenantBySlug(string slug);
    public Tenant? GetTenantById(Guid id);
    public void AddTenant(Tenant tenant);
}
