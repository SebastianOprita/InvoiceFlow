using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IUsersRepository
{
    public Task<List<User>> GetAllUsersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    public Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    public Task<User?> GetUserByEmailAsync(Guid tenantId, UserEmail email, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByEmailAsync(Guid tenantId, UserEmail email, CancellationToken cancellationToken = default);
    public Task<User?> GetUserByIdWithPermissionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    public Task<User?> GetTrackedUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    public Task<User?> GetTrackedUserByEmailAsync(Guid tenantId, UserEmail email, CancellationToken cancellationToken = default);
    public void AddUser(User user);
}
