using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class TenantsRepository(IdentityDbContext dbContext) : ITenantsRepository
{
    public async Task<bool> ExistsBySlugAsync(TenantSlug slug)
    {
        return await dbContext.Tenants.AsNoTracking().AnyAsync(t => t.Slug == slug);
    }

    public async Task<List<Tenant>> FindAllTenantsAsync()
    {
        return await dbContext.Tenants.AsNoTracking().ToListAsync();
    }

    public async Task<Tenant?> FindTenantByIdAsync(Guid id)
    {
        return await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> FindTenantBySlugAsync(string slug)
    {
        return await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug.Value == slug);
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid id)
    {
        return await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id);
    }

    public void AddTenant(Tenant tenant)
    {
        dbContext.Tenants.Add(tenant);
    }
}
