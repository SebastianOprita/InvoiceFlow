using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class RolesRepository(IdentityDbContext dbContext) : IRolesRepository
{
    public List<Role> FindAllRoles(Guid tenantId)
    {
        return dbContext.Roles.AsNoTracking().Where(r => r.TenantId == tenantId).ToList();
    }
    public Role? FindRoleById(Guid tenantId, Guid roleId)
    {
        return dbContext.Roles.AsNoTracking().FirstOrDefault(r => r.TenantId == tenantId && r.Id == roleId);
    }
    public Role? GetRoleById(Guid tenantId, Guid roleId)
    {
        return dbContext.Roles.FirstOrDefault(r => r.TenantId == tenantId && r.Id == roleId);
    }
    public bool ExistsByName(Guid tenantId, RoleName name)
    {
        return dbContext.Roles.AsNoTracking().Any(r => r.TenantId == tenantId && r.Name == name);
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
