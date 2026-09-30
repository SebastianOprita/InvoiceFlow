using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformRefreshTokensRepository
{
    public Task<PlatformRefreshToken?> FindPlatformRefreshTokenAsync(RefreshTokenHash tokenHash);
    public Task<PlatformRefreshToken?> GetPlatformRefreshTokenAsync(RefreshTokenHash tokenHash);
    public void AddPlatformRefreshToken(PlatformRefreshToken refreshToken);
    public Task RevokeAccessForPlatformUserAsync(
        Guid userId,
        DateTime revokedAtUtc);
}
