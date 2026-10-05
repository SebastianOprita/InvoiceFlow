using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class PlatformUsersRepository(IdentityDbContext dbContext) : IPlatformUsersRepository
{
    public async Task<List<PlatformUser>> FindAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformUsers.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<PlatformUser?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<PlatformUser?> FindUserByEmailAsync(UserEmail email, CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformUsers.AsNoTracking().AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<PlatformUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformUsers.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<PlatformUser?> GetUserByEmailAsync(UserEmail email, CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformUsers.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public void AddUser(PlatformUser user)
    {
        dbContext.PlatformUsers.Add(user);
    }
}
