using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IUsersRepository
{
    public Task<List<User>> FindAllUsersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    public Task<User?> FindUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    public Task<User?> FindUserByEmailAsync(Guid tenantId, UserEmail email, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByEmailAsync(Guid tenantId, UserEmail email, CancellationToken cancellationToken = default);
    public Task<User?> FindUserByIdWithPermissionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    public Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    public Task<User?> GetUserByEmailAsync(Guid tenantId, UserEmail email, CancellationToken cancellationToken = default);
    public void AddUser(User user);
}
