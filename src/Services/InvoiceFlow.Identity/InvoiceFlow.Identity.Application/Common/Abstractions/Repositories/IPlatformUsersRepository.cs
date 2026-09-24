using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformUsersRepository
{
    public List<PlatformUser> FindAllUsers();
    public PlatformUser? FindUserById(Guid userId);
    public PlatformUser? FindUserByEmail(UserEmail email);
    public bool ExistsByEmail(UserEmail email);
    public PlatformUser? GetUserById(Guid userId);
    public PlatformUser? GetUserByEmail(UserEmail email);
    public void AddUser(PlatformUser user);
}
