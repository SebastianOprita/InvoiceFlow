using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IRefreshTokensRepository
{
    public Task<RefreshToken?> FindRefreshTokenAsync(Guid tenantId, RefreshTokenHash tokenHash);
    public Task<RefreshToken?> GetRefreshTokenAsync(Guid tenantId, RefreshTokenHash tokenHash);
    public void AddRefreshToken(RefreshToken refreshToken);
    public Task RevokeAccessForUserAsync(
        Guid tenantId,
        Guid userId,
        DateTime revokedAtUtc);
}
