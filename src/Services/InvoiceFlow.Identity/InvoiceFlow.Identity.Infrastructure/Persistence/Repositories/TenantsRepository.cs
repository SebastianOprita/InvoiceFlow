using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class TenantsRepository(IdentityDbContext dbContext) : ITenantsRepository
{
    public bool ExistsBySlug(TenantSlug slug)
    {
        return dbContext.Tenants.AsNoTracking().Any(t => t.Slug == slug);
    }

    public List<Tenant> FindAllTenants()
    {
        return dbContext.Tenants.AsNoTracking().ToList();
    }

    public Tenant? FindTenantById(Guid id)
    {
        return dbContext.Tenants.AsNoTracking().FirstOrDefault(t => t.Id == id);
    }

    public Tenant? FindTenantBySlug(string slug)
    {
        return dbContext.Tenants.AsNoTracking().FirstOrDefault(t => t.Slug.Value == slug);
    }

    public Tenant? GetTenantById(Guid id)
    {
        return dbContext.Tenants.FirstOrDefault(t => t.Id == id);
    }

    public void AddTenant(Tenant tenant)
    {
        dbContext.Tenants.Add(tenant);
    }
}
