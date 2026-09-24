using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IRefreshTokensRepository
{
    public RefreshToken? FindRefreshToken(Guid tenantId, RefreshTokenHash tokenHash);
    public RefreshToken? GetRefreshToken(Guid tenantId, RefreshTokenHash tokenHash);
    public void AddRefreshToken(RefreshToken refreshToken);
    public Task RevokeAccessForUserAsync(
        Guid tenantId,
        Guid userId,
        DateTime revokedAtUtc);
}
