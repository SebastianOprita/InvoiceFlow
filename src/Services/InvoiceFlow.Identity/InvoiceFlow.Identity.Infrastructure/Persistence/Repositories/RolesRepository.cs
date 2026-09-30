using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Identity.Infrastructure;

[ExcludeFromCodeCoverage]
public class RolesRepository(IdentityDbContext dbContext) : IRolesRepository
{
    public async Task<List<Role>> FindAllRolesAsync(Guid tenantId)
    {
        var roles = await dbContext.Roles.AsNoTracking().Where(r => r.TenantId == tenantId).ToListAsync();
        return roles;
    }
    public async Task<Role?> FindRoleByIdAsync(Guid tenantId, Guid roleId)
    {
        var role = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId);
        return role;
    }
    public async Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId)
    {
        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId);
        return role;
    }
    public async Task<bool> ExistsByNameAsync(Guid tenantId, RoleName name)
    {
        return await dbContext.Roles.AsNoTracking().AnyAsync(r => r.TenantId == tenantId && r.Name == name);
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
