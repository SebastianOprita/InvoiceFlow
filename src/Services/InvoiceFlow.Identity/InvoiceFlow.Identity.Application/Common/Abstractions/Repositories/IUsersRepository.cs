using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IUsersRepository
{
    public List<User> FindAllUsers(Guid tenantId);
    public User? FindUserById(Guid tenantId, Guid userId);
    public User? FindUserByEmail(Guid tenantId, UserEmail email);
    public bool ExistsByEmail(Guid tenantId, UserEmail email);
    public User? FindUserByIdWithPermissions(Guid tenantId, Guid userId);
    public User? GetUserById(Guid tenantId, Guid userId);
    public User? GetUserByEmail(Guid tenantId, UserEmail email);
    public void AddUser(User user);
}
