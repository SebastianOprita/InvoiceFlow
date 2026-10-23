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
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired(DateTimeOffset now) => now.ToUniversalTime() >= ExpiresAtUtc;
    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now.ToUniversalTime());

#pragma warning disable CS8618
    private PlatformRefreshToken() { } // EF Core
#pragma warning restore CS8618

    private PlatformRefreshToken(
        Guid id,
        Guid userId,
        RefreshTokenHash tokenHash,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc,
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
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc,
        DeviceInfo? deviceInfo = null,
        IpAddress? ipAddress = null)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (userId == Guid.Empty)
            throw new DomainException(DomainErrors.UserIdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (expiresAtUtc == default)
            throw new DomainException(DomainErrors.ExpiresAtUtcRequired);

        if (expiresAtUtc <= createdAtUtc)
            throw new DomainException(DomainErrors.ExpiresAtUtcInvalid);

        return new PlatformRefreshToken(
            id,
            userId,
            tokenHash,
            expiresAtUtc.ToUniversalTime(),
            createdAtUtc.ToUniversalTime(),
            deviceInfo,
            ipAddress);
    }

    public void Revoke(DateTimeOffset revokedAtUtc)
    {
        if (IsRevoked)
            return;

        if (revokedAtUtc == default)
            throw new DomainException(DomainErrors.RevokedAtUtcRequired);

        revokedAtUtc = revokedAtUtc.ToUniversalTime();

        if (revokedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.RevokedAtUtcInvalid);

        RevokedAtUtc = revokedAtUtc;
    }

    public PlatformRefreshToken Rotate(
        RefreshTokenHash newTokenHash,
        DateTimeOffset newExpiresAtUtc,
        DateTimeOffset createdAtUtc,
        DeviceInfo? deviceInfo = null,
        IpAddress? ipAddress = null)
    {
        if (IsRevoked)
            throw new DomainException(DomainErrors.RefreshTokensRevoked);

        if (IsExpired(createdAtUtc))
            throw new DomainException(DomainErrors.RefreshTokensExpired);

        var newToken = PlatformRefreshToken.Create(
            Guid.CreateVersion7(),
            UserId,
            newTokenHash,
            newExpiresAtUtc,
            createdAtUtc,
            deviceInfo,
            ipAddress);

        RevokedAtUtc = createdAtUtc.ToUniversalTime();
        ReplacedByTokenId = newToken.Id;

        return newToken;
    }
}
