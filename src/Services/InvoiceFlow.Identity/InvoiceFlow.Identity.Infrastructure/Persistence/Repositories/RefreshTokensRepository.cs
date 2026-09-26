using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class RefreshTokensRepository(IdentityDbContext dbContext) : IRefreshTokensRepository
{
    public RefreshToken? FindRefreshToken(Guid tenantId, RefreshTokenHash tokenHash)
    {
        var refreshToken = dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefault(t =>
                t.TenantId == tenantId && t.TokenHash == tokenHash);

        return refreshToken;
    }

    public RefreshToken? GetRefreshToken(Guid tenantId, RefreshTokenHash tokenHash)
    {
        var refreshToken = dbContext.RefreshTokens
            .FirstOrDefault(t =>
                t.TenantId == tenantId && t.TokenHash == tokenHash);

        return refreshToken;
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        dbContext.RefreshTokens.Add(refreshToken);
    }

    public async Task RevokeAccessForUserAsync(
        Guid tenantId,
        Guid userId,
        DateTime revokedAtUtc)
    {
        await dbContext.RefreshTokens
            .Where(t =>
                t.TenantId == tenantId &&
                t.UserId == userId &&
                t.RevokedAtUtc == null &&
                t.ExpiresAtUtc > revokedAtUtc)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(t => t.RevokedAtUtc, revokedAtUtc));
    }
}
