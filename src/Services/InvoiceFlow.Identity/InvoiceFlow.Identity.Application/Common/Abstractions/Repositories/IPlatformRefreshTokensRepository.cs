using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformRefreshTokensRepository
{
    public Task<PlatformRefreshToken?> FindPlatformRefreshTokenAsync(RefreshTokenHash tokenHash, CancellationToken cancellationToken = default);
    public Task<PlatformRefreshToken?> GetPlatformRefreshTokenAsync(RefreshTokenHash tokenHash, CancellationToken cancellationToken = default);
    public void AddPlatformRefreshToken(PlatformRefreshToken refreshToken);
    public Task RevokeAccessForPlatformUserAsync(
        Guid userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default);
}
