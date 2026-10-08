using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class RefreshTokensRepository(IdentityDbContext dbContext) : IRefreshTokensRepository
{
    public Task<RefreshToken?> GetRefreshTokenAsync(Guid tenantId, RefreshTokenHash tokenHash, CancellationToken cancellationToken = default)
    {
        var refreshToken = dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.TenantId == tenantId && t.TokenHash == tokenHash, cancellationToken);

        return refreshToken;
    }

    public Task<RefreshToken?> GetTrackedRefreshTokenAsync(Guid tenantId, RefreshTokenHash tokenHash, CancellationToken cancellationToken = default)
    {
        var refreshToken = dbContext.RefreshTokens
            .FirstOrDefaultAsync(t =>
                t.TenantId == tenantId && t.TokenHash == tokenHash, cancellationToken);

        return refreshToken;
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        dbContext.RefreshTokens.Add(refreshToken);
    }

    public async Task RevokeAccessForUserAsync(
        Guid tenantId,
        Guid userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await dbContext.RefreshTokens
        .Where(t =>
            t.TenantId == tenantId &&
            t.UserId == userId &&
            t.RevokedAtUtc == null &&
            t.ExpiresAtUtc > revokedAtUtc)
        .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedAtUtc);
        }
    }
}
