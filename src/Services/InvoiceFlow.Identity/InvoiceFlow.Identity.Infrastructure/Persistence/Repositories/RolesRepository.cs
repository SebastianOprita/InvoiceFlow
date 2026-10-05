using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class RolesRepository(IdentityDbContext dbContext) : IRolesRepository
{
    public async Task<List<Role>> FindAllRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles.AsNoTracking().Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken);
        return roles;
    }
    public async Task<Role?> FindRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId, cancellationToken);
        return role;
    }
    public async Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId, cancellationToken);
        return role;
    }
    public async Task<bool> ExistsByNameAsync(Guid tenantId, RoleName name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles.AsNoTracking().AnyAsync(r => r.TenantId == tenantId && r.Name == name, cancellationToken);
    }
    public void AddRole(Role role)
    {
        dbContext.Roles.Add(role);
    }
    public void RemoveRole(Role role) 
    {
        dbContext.Roles.Remove(role);
    }
}
