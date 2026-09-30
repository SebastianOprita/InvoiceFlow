using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Identity.Infrastructure;

[ExcludeFromCodeCoverage]
public class RefreshTokensRepository(IdentityDbContext dbContext) : IRefreshTokensRepository
{
    public Task<RefreshToken?> FindRefreshTokenAsync(Guid tenantId, RefreshTokenHash tokenHash)
    {
        var refreshToken = dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.TenantId == tenantId && t.TokenHash == tokenHash);

        return refreshToken;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(Guid tenantId, RefreshTokenHash tokenHash)
    {
        var refreshToken = dbContext.RefreshTokens
            .FirstOrDefaultAsync(t =>
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
