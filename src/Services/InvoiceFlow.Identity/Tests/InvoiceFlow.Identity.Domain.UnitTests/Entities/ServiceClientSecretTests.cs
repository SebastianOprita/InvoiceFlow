using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public sealed class ServiceClientSecretTests
{
    private static readonly DateTimeOffset CreatedAtUtc = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ExpiresAtUtc = CreatedAtUtc.AddDays(30);

    [Fact]
    public void Create_StoresValuesAndNormalizesTimestamps()
    {
        var secret = CreateSecret();

        secret.Id.Should().NotBeEmpty();
        secret.ServiceClientId.Should().NotBeEmpty();
        secret.SecretHash.Value.Should().Be(new string('A', 64));
        secret.Name.Value.Should().Be("deploy");
        secret.Description.Should().BeNull();
        secret.CreatedAtUtc.Should().Be(CreatedAtUtc);
        secret.ExpiresAtUtc.Should().Be(ExpiresAtUtc);
        secret.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_RejectsInvalidIdentifiersHashNameAndExpiration()
    {
        FluentActions.Invoking(() => ServiceClientSecret.Create(Guid.Empty, Guid.NewGuid(), Hash(), Name(), null, CreatedAtUtc, ExpiresAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
        FluentActions.Invoking(() => ServiceClientSecret.Create(Guid.NewGuid(), Guid.Empty, Hash(), Name(), null, CreatedAtUtc, ExpiresAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.ServiceClientIdInvalid);
        FluentActions.Invoking(() => ServiceClientSecret.Create(Guid.NewGuid(), Guid.NewGuid(), null!, Name(), null, CreatedAtUtc, ExpiresAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.ServiceClientSecretHashRequired);
        FluentActions.Invoking(() => ServiceClientSecret.Create(Guid.NewGuid(), Guid.NewGuid(), Hash(), null!, null, CreatedAtUtc, ExpiresAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.ServiceClientSecretNameRequired);
        FluentActions.Invoking(() => ServiceClientSecret.Create(Guid.NewGuid(), Guid.NewGuid(), Hash(), Name(), null, CreatedAtUtc, CreatedAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.ExpiresAtUtcInvalid);
    }

    [Fact]
    public void IsActive_ReturnsFalseWhenExpiredOrRevoked()
    {
        var secret = CreateSecret();

        secret.IsActive(CreatedAtUtc).Should().BeTrue();
        secret.IsActive(ExpiresAtUtc).Should().BeFalse();

        secret.Revoke(CreatedAtUtc.AddDays(1));
        secret.IsActive(CreatedAtUtc.AddDays(1)).Should().BeFalse();
        secret.RevokedAtUtc.Should().Be(CreatedAtUtc.AddDays(1));
    }

    [Fact]
    public void Revoke_IsIdempotentAndRejectsTimeBeforeCreation()
    {
        var secret = CreateSecret();
        FluentActions.Invoking(() => secret.Revoke(CreatedAtUtc.AddTicks(-1)))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.RevokedAtUtcInvalid);

        var revokedAt = CreatedAtUtc.AddDays(1);
        secret.Revoke(revokedAt);
        secret.Revoke(revokedAt.AddDays(1));
        secret.RevokedAtUtc.Should().Be(revokedAt);
    }

    private static ServiceClientSecret CreateSecret() =>
        ServiceClientSecret.Create(Guid.NewGuid(), Guid.NewGuid(), Hash(), Name(), null, CreatedAtUtc, ExpiresAtUtc);

    private static ServiceClientSecretHash Hash() => ServiceClientSecretHash.Create(new string('A', 64));

    private static ServiceClientSecretName Name() => ServiceClientSecretName.Create("deploy");
}
