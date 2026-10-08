using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

public class PlatformRefreshTokensRepository(IdentityDbContext dbContext) : IPlatformRefreshTokensRepository
{
    public async Task<PlatformRefreshToken?> GetPlatformRefreshTokenAsync(RefreshTokenHash tokenHash, CancellationToken cancellationToken = default)
    {
        var refreshToken = await dbContext.PlatformRefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        return refreshToken;
    }

    public async Task<PlatformRefreshToken?> GetTrackedPlatformRefreshTokenAsync(RefreshTokenHash tokenHash, CancellationToken cancellationToken = default)
    {
        var refreshToken = await dbContext.PlatformRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        return refreshToken;
    }

    public void AddPlatformRefreshToken(PlatformRefreshToken refreshToken)
    {
        dbContext.PlatformRefreshTokens.Add(refreshToken);
    }

    public async Task RevokeAccessForPlatformUserAsync(
        Guid userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await dbContext.PlatformRefreshTokens
        .Where(t =>
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
