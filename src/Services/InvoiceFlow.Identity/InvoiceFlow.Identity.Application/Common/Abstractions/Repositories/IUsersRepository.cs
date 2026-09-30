using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IUsersRepository
{
    public Task<List<User>> FindAllUsersAsync(Guid tenantId);
    public Task<User?> FindUserByIdAsync(Guid tenantId, Guid userId);
    public Task<User?> FindUserByEmailAsync(Guid tenantId, UserEmail email);
    public Task<bool> ExistsByEmailAsync(Guid tenantId, UserEmail email);
    public Task<User?> FindUserByIdWithPermissionsAsync(Guid tenantId, Guid userId);
    public Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId);
    public Task<User?> GetUserByEmailAsync(Guid tenantId, UserEmail email);
    public void AddUser(User user);
}
