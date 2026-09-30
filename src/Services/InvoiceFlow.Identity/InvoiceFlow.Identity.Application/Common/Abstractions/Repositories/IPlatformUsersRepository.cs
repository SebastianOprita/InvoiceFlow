using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformUsersRepository
{
    public Task<List<PlatformUser>> FindAllUsersAsync();
    public Task<PlatformUser?> FindUserByIdAsync(Guid userId);
    public Task<PlatformUser?> FindUserByEmailAsync(UserEmail email);
    public Task<bool> ExistsByEmailAsync(UserEmail email);
    public Task<PlatformUser?> GetUserByIdAsync(Guid userId);
    public Task<PlatformUser?> GetUserByEmailAsync(UserEmail email);
    public void AddUser(PlatformUser user);
}
