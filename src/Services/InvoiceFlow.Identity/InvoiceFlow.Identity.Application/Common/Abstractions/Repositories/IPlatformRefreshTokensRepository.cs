using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IPlatformRefreshTokensRepository
{
    public PlatformRefreshToken? FindPlatformRefreshToken(RefreshTokenHash tokenHash);
    public PlatformRefreshToken? GetPlatformRefreshToken(RefreshTokenHash tokenHash);
    public void AddPlatformRefreshToken(PlatformRefreshToken refreshToken);
    public Task RevokeAccessForPlatformUserAsync(
        Guid userId,
        DateTime revokedAtUtc);
}
