using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class UsersRepository(IdentityDbContext dbContext) : IUsersRepository
{
    public List<User> FindAllUsers(Guid tenantId)
    {
        return dbContext.Users.AsNoTracking().Where(u => u.TenantId == tenantId).ToList();
    }

    public User? FindUserById(Guid tenantId, Guid userId)
    {
        return dbContext.Users.AsNoTracking().FirstOrDefault(u => u.TenantId == tenantId && u.Id == userId);
    }

    public User? FindUserByEmail(Guid tenantId, UserEmail email)
    {
        return dbContext.Users.AsNoTracking().FirstOrDefault(u => u.TenantId == tenantId && u.Email == email);
    }

    public bool ExistsByEmail(Guid tenantId, UserEmail email)
    {
        return dbContext.Users.AsNoTracking().Any(u => u.TenantId == tenantId && u.Email == email);
    }

    public User? FindUserByIdWithPermissions(Guid tenantId, Guid userId)
    {
        return dbContext.Users.AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.TenantId == tenantId && u.Id == userId && u.IsActive);
    }

    public User? GetUserById(Guid tenantId, Guid userId)
    {
        return dbContext.Users.FirstOrDefault(u => u.TenantId == tenantId && u.Id == userId);
    }

    public User? GetUserByEmail(Guid tenantId, UserEmail email)
    {
        return dbContext.Users.FirstOrDefault(u => u.TenantId == tenantId && u.Email == email);
    }

    public void AddUser(User user)
    {
        dbContext.Users.Add(user);
    }
}
