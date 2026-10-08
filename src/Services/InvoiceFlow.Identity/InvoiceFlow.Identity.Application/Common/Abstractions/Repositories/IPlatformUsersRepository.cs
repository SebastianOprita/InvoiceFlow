using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformUsersRepository
{
    public Task<List<PlatformUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    public Task<PlatformUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<PlatformUser?> GetUserByEmailAsync(UserEmail email, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken = default);
    public Task<PlatformUser?> GetTrackedUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<PlatformUser?> GetTrackedUserByEmailAsync(UserEmail email, CancellationToken cancellationToken = default);
    public void AddUser(PlatformUser user);
}
