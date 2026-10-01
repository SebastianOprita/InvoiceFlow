using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class UsersRepository(IdentityDbContext dbContext) : IUsersRepository
{
    public async Task<List<User>> FindAllUsersAsync(Guid tenantId)
    {
        return await dbContext.Users.AsNoTracking().Where(u => u.TenantId == tenantId).ToListAsync();
    }

    public async Task<User?> FindUserByIdAsync(Guid tenantId, Guid userId)
    {
        return await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId);
    }

    public async Task<User?> FindUserByEmailAsync(Guid tenantId, UserEmail email)
    {
        return await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(Guid tenantId, UserEmail email)
    {
        return await dbContext.Users.AsNoTracking().AnyAsync(u => u.TenantId == tenantId && u.Email == email);
    }

    public async Task<User?> FindUserByIdWithPermissionsAsync(Guid tenantId, Guid userId)
    {
        return await dbContext.Users.AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && u.IsActive);
    }

    public async Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(Guid tenantId, UserEmail email)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email);
    }

    public void AddUser(User user)
    {
        dbContext.Users.Add(user);
    }
}
