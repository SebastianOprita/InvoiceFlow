using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IRolesRepository
{
    public Task<List<Role>> FindAllRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    public Task<Role?> FindRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
    public Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(Guid tenantId, RoleName name, CancellationToken cancellationToken = default);
    public void AddRole(Role role);
    public void RemoveRole(Role role);
}
