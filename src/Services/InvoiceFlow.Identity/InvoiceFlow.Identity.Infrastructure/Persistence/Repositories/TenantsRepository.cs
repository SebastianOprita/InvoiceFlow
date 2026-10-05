using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class TenantsRepository(IdentityDbContext dbContext) : ITenantsRepository
{
    public async Task<bool> ExistsBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants.AsNoTracking().AnyAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<List<Tenant>> FindAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> FindTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> FindTenantBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug.Value == slug, cancellationToken);
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public void AddTenant(Tenant tenant)
    {
        dbContext.Tenants.Add(tenant);
    }
}
