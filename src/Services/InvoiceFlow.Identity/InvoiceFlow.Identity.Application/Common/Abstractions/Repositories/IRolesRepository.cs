using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IRolesRepository
{
    public Task<List<Role>> FindAllRolesAsync(Guid tenantId);
    public Task<Role?> FindRoleByIdAsync(Guid tenantId, Guid roleId);
    public Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId);
    public Task<bool> ExistsByNameAsync(Guid tenantId, RoleName name);
    public void AddRole(Role role);
    public void RemoveRole(Role role);
}
