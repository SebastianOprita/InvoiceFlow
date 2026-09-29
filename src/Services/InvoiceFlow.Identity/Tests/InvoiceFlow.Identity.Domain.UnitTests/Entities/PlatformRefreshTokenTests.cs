using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public sealed class PlatformRefreshTokenTests
{
    private static readonly Guid TokenId = Guid.CreateVersion7();
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly DateTime DateTime = DateTime.UtcNow;

    [Fact]
    public void Constructor_WithValidValues_ShouldCreateRefreshToken()
    {
        var createdAt = DateTime;
        var expiresAt = createdAt.AddDays(7);
        var tokenHash = RefreshTokenHash.Create("token-hash");
        var deviceInfo = DeviceInfo.Create("Chrome");
        var ipAddress = IpAddress.Create("127.0.0.1");

        var token = PlatformRefreshToken.Create(
            TokenId,
            UserId,
            tokenHash,
            expiresAt,
            createdAt,
            deviceInfo,
            ipAddress);

        token.Id.Should().Be(TokenId);
        token.UserId.Should().Be(UserId);
        token.TokenHash.Should().Be(tokenHash);
        token.ExpiresAtUtc.Should().Be(expiresAt);
        token.CreatedAtUtc.Should().Be(createdAt);
        token.DeviceInfo.Should().Be(deviceInfo);
        token.IpAddress.Should().Be(ipAddress);
        token.RevokedAtUtc.Should().BeNull();
        token.ReplacedByTokenId.Should().BeNull();
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowDomainException()
    {
        var act = () => PlatformRefreshToken.Create(
            Guid.Empty,
            UserId,
            RefreshTokenHash.Create("token-hash"),
            DateTime.AddDays(1),
            DateTime);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.IdRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowDomainException()
    {
        var act = () => PlatformRefreshToken.Create(
            TokenId,
            Guid.Empty,
            RefreshTokenHash.Create("token-hash"),
            DateTime.AddDays(1),
            DateTime);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.UserIdRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_WithDefaultCreatedAtUtc_ShouldThrowDomainException()
    {
        var act = () => PlatformRefreshToken.Create(
            TokenId,
            UserId,
            RefreshTokenHash.Create("token-hash"),
            DateTime.AddDays(1),
            default);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CreatedAtUtcRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_WithExpiresAtBeforeCreatedAt_ShouldThrowDomainException()
    {
        var createdAt = DateTime;

        var act = () => PlatformRefreshToken.Create(
            TokenId,
            UserId,
            RefreshTokenHash.Create("token-hash"),
            createdAt.AddSeconds(-1),
            createdAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.ExpiresAtUtcInvalid.ErrorMessage);
    }

    [Fact]
    public void Constructor_WithExpiresAtEqualToCreatedAt_ShouldThrowDomainException()
    {
        var createdAt = DateTime;

        var act = () => PlatformRefreshToken.Create(
            TokenId,
            UserId,
            RefreshTokenHash.Create("token-hash"),
            createdAt,
            createdAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.ExpiresAtUtcInvalid.ErrorMessage);
    }

    [Fact]
    public void IsExpired_WhenNowIsBeforeExpiresAt_ShouldReturnFalse()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));

        token.IsExpired(createdAt.AddHours(12)).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenNowIsEqualToExpiresAt_ShouldReturnTrue()
    {
        var createdAt = DateTime;
        var expiresAt = createdAt.AddDays(1);
        var token = CreateToken(createdAt, expiresAt);

        token.IsExpired(expiresAt).Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenTokenIsNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));

        token.IsActive(createdAt.AddHours(1)).Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenTokenIsExpired_ShouldReturnFalse()
    {
        var createdAt = DateTime;
        var expiresAt = createdAt.AddDays(1);
        var token = CreateToken(createdAt, expiresAt);

        token.IsActive(expiresAt).Should().BeFalse();
    }

    [Fact]
    public void Revoke_WithValidDate_ShouldRevokeToken()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));
        var revokedAt = createdAt.AddHours(1);

        token.Revoke(revokedAt);

        token.IsRevoked.Should().BeTrue();
        token.RevokedAtUtc.Should().Be(revokedAt);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldDoNothing()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));
        var firstRevokedAt = createdAt.AddHours(1);
        var secondRevokedAt = createdAt.AddHours(2);

        token.Revoke(firstRevokedAt);
        token.Revoke(secondRevokedAt);

        token.RevokedAtUtc.Should().Be(firstRevokedAt);
    }

    [Fact]
    public void Revoke_WithDefaultDate_ShouldThrowDomainException()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));

        var act = () => token.Revoke(default);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RevokedAtUtcRequired.ErrorMessage);
    }

    [Fact]
    public void Revoke_WithDateEarlierThanCreatedAt_ShouldThrowDomainException()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));

        var act = () => token.Revoke(createdAt.AddSeconds(-1));

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RevokedAtUtcInvalid.ErrorMessage);
    }

    [Fact]
    public void Rotate_WithValidValues_ShouldRevokeCurrentTokenAndReturnNewToken()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));
        var rotatedAt = createdAt.AddHours(1);
        var newExpiresAt = rotatedAt.AddDays(7);
        var newTokenHash = RefreshTokenHash.Create("new-token-hash");
        var deviceInfo = DeviceInfo.Create("Firefox");
        var ipAddress = IpAddress.Create("10.0.0.1");

        var newToken = token.Rotate(
            newTokenHash,
            newExpiresAt,
            rotatedAt,
            deviceInfo,
            ipAddress);

        token.IsRevoked.Should().BeTrue();
        token.RevokedAtUtc.Should().Be(rotatedAt);
        token.ReplacedByTokenId.Should().Be(newToken.Id);

        newToken.UserId.Should().Be(token.UserId);
        newToken.TokenHash.Should().Be(newTokenHash);
        newToken.ExpiresAtUtc.Should().Be(newExpiresAt);
        newToken.CreatedAtUtc.Should().Be(rotatedAt);
        newToken.DeviceInfo.Should().Be(deviceInfo);
        newToken.IpAddress.Should().Be(ipAddress);
        newToken.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Rotate_WhenTokenIsRevoked_ShouldThrowDomainException()
    {
        var createdAt = DateTime;
        var token = CreateToken(createdAt, createdAt.AddDays(1));
        token.Revoke(createdAt.AddHours(1));

        var act = () => token.Rotate(
            RefreshTokenHash.Create("new-token-hash"),
            createdAt.AddDays(2),
            createdAt.AddHours(2));

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RefreshTokensRevoked.ErrorMessage);
    }

    [Fact]
    public void Rotate_WhenTokenIsExpired_ShouldThrowDomainException()
    {
        var createdAt = DateTime;
        var expiresAt = createdAt.AddDays(1);
        var token = CreateToken(createdAt, expiresAt);

        var act = () => token.Rotate(
            RefreshTokenHash.Create("new-token-hash"),
            expiresAt.AddDays(7),
            expiresAt);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RefreshTokensExpired.ErrorMessage);
    }

    private static PlatformRefreshToken CreateToken(DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        return PlatformRefreshToken.Create(
            TokenId,
            UserId,
            RefreshTokenHash.Create("token-hash"),
            expiresAtUtc,
            createdAtUtc);
    }
}
