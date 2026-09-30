using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Identity.Infrastructure;

[ExcludeFromCodeCoverage]
public class PlatformUsersRepository(IdentityDbContext dbContext) : IPlatformUsersRepository
{
    public async Task<List<PlatformUser>> FindAllUsersAsync()
    {
        return await dbContext.PlatformUsers.AsNoTracking().ToListAsync();
    }

    public async Task<PlatformUser?> FindUserByIdAsync(Guid userId)
    {
        return await dbContext.PlatformUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<PlatformUser?> FindUserByEmailAsync(UserEmail email)
    {
        return await dbContext.PlatformUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(UserEmail email)
    {
        return await dbContext.PlatformUsers.AsNoTracking().AnyAsync(u => u.Email == email);
    }

    public async Task<PlatformUser?> GetUserByIdAsync(Guid userId)
    {
        return await dbContext.PlatformUsers.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<PlatformUser?> GetUserByEmailAsync(UserEmail email)
    {
        return await dbContext.PlatformUsers.FirstOrDefaultAsync(u => u.Email == email);
    }

    public void AddUser(PlatformUser user)
    {
        dbContext.PlatformUsers.Add(user);
    }
}
