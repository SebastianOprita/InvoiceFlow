using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IRolesRepository
{
    public List<Role> FindAllRoles(Guid tenantId);
    public Role? FindRoleById(Guid tenantId, Guid roleId);
    public Role? GetRoleById(Guid tenantId, Guid roleId);
    public bool ExistsByName(Guid tenantId, RoleName name);
    public void AddRole(Role role);
    public void RemoveRole(Role role);
}
