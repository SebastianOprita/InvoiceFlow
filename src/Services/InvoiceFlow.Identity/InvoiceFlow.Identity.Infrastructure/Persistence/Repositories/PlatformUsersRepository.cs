using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class PlatformUsersRepository(IdentityDbContext dbContext) : IPlatformUsersRepository
{
    public List<PlatformUser> FindAllUsers()
    {
        return dbContext.PlatformUsers.AsNoTracking().ToList();
    }

    public PlatformUser? FindUserById(Guid userId)
    {
        return dbContext.PlatformUsers.AsNoTracking().FirstOrDefault(u => u.Id == userId);
    }

    public PlatformUser? FindUserByEmail(UserEmail email)
    {
        return dbContext.PlatformUsers.AsNoTracking().FirstOrDefault(u => u.Email == email);
    }

    public bool ExistsByEmail(UserEmail email)
    {
        return dbContext.PlatformUsers.AsNoTracking().Any(u => u.Email == email);
    }

    public PlatformUser? GetUserById(Guid userId)
    {
        return dbContext.PlatformUsers.FirstOrDefault(u => u.Id == userId);
    }

    public PlatformUser? GetUserByEmail(UserEmail email)
    {
        return dbContext.PlatformUsers.FirstOrDefault(u => u.Email == email);
    }

    public void AddUser(PlatformUser user)
    {
        dbContext.PlatformUsers.Add(user);
    }
}
