using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformUsersRepository
{
    public Task<List<PlatformUser>> FindAllUsersAsync(CancellationToken cancellationToken = default);
    public Task<PlatformUser?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<PlatformUser?> FindUserByEmailAsync(UserEmail email, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken = default);
    public Task<PlatformUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<PlatformUser?> GetUserByEmailAsync(UserEmail email, CancellationToken cancellationToken = default);
    public void AddUser(PlatformUser user);
}
