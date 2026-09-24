using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class PlatformRefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public RefreshTokenHash TokenHash { get; private set; }
    public DeviceInfo? DeviceInfo { get; private set; }
    public IpAddress? IpAddress { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired(DateTime now) => now >= ExpiresAtUtc;
    public bool IsActive(DateTime now) => !IsRevoked && !IsExpired(now);

#pragma warning disable CS8618
    private PlatformRefreshToken() { } // EF Core
#pragma warning restore CS8618

    private PlatformRefreshToken(
        Guid id,
        Guid userId,
        RefreshTokenHash tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        DeviceInfo? deviceInfo = null,
        IpAddress? ipAddress = null)
    {

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        DeviceInfo = deviceInfo;
        IpAddress = ipAddress;
        CreatedAtUtc = createdAtUtc;
    }

    public static PlatformRefreshToken Create(
        Guid id,
        Guid userId,
        RefreshTokenHash tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        DeviceInfo? deviceInfo = null,
        IpAddress? ipAddress = null)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (userId == Guid.Empty)
            throw new DomainException(DomainErrors.UserIdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        if (expiresAtUtc == default)
            throw new DomainException(DomainErrors.ExpiresAtUtcRequired);

        if (expiresAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.ExpiresAtUtcNotUtc);

        if (expiresAtUtc <= createdAtUtc)
            throw new DomainException(DomainErrors.ExpiresAtUtcInvalid);

        return new PlatformRefreshToken(
            id,
            userId,
            tokenHash,
            expiresAtUtc,
            createdAtUtc,
            deviceInfo,
            ipAddress);
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        if (IsRevoked)
            return;

        if (revokedAtUtc == default)
            throw new DomainException(DomainErrors.RevokedAtUtcRequired);

        if (revokedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.RevokedAtUtcNotUtc);

        if (revokedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.RevokedAtUtcInvalid);

        RevokedAtUtc = revokedAtUtc;
    }

    public PlatformRefreshToken Rotate(
        RefreshTokenHash newTokenHash,
        DateTime newExpiresAtUtc,
        DateTime createdAtUtc,
        DeviceInfo? deviceInfo = null,
        IpAddress? ipAddress = null)
    {
        if (IsRevoked)
            throw new DomainException(DomainErrors.RefreshTokensRevoked);

        if (IsExpired(createdAtUtc))
            throw new DomainException(DomainErrors.RefreshTokensExpired);

        var newToken = new PlatformRefreshToken(
            Guid.CreateVersion7(),
            UserId,
            newTokenHash,
            newExpiresAtUtc,
            createdAtUtc,
            deviceInfo,
            ipAddress);

        RevokedAtUtc = createdAtUtc;
        ReplacedByTokenId = newToken.Id;

        return newToken;
    }
}
