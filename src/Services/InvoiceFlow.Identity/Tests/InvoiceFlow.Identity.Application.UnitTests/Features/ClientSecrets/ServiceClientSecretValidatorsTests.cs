using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.ClientSecrets;

public sealed class ServiceClientSecretValidatorsTests
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateSecretValidator_ValidatesRequiredFieldsAndMaximumLengths()
    {
        var validator = new CreateServiceClientSecretCommandValidator();
        validator.Validate(new CreateServiceClientSecretCommand(ClientId, "key", "optional", Now.AddDays(1))).IsValid.Should().BeTrue();
        validator.Validate(new CreateServiceClientSecretCommand(Guid.Empty, "", new string('x', ServiceClientSecretDescription.MaxLength + 1), default)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void GrantPermissionsValidator_RequiresExactlyOneKnownPermission()
    {
        var validator = new GrantServiceClientPermissionsCommandValidator();
        validator.Validate(new GrantServiceClientPermissionsCommand(ClientId, 1L << 3)).IsValid.Should().BeTrue();
        validator.Validate(new GrantServiceClientPermissionsCommand(Guid.Empty, 0)).IsValid.Should().BeFalse();
        validator.Validate(new GrantServiceClientPermissionsCommand(ClientId, (1L << 3) | (1L << 4))).IsValid.Should().BeFalse();
    }

    [Fact]
    public void RevokePermissionsValidator_RequiresExactlyOneKnownPermission()
    {
        var validator = new RevokeServiceClientPermissionsCommandValidator();
        validator.Validate(new RevokeServiceClientPermissionsCommand(ClientId, 1L << 3)).IsValid.Should().BeTrue();
        validator.Validate(new RevokeServiceClientPermissionsCommand(ClientId, -1)).IsValid.Should().BeFalse();
        validator.Validate(new RevokeServiceClientPermissionsCommand(Guid.Empty, 1L << 40)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void TenantCommandValidators_RequireBothIdentifiers()
    {
        new GrantServiceClientTenantCommandValidator().Validate(new GrantServiceClientTenantCommand(ClientId, TenantId)).IsValid.Should().BeTrue();
        new GrantServiceClientTenantCommandValidator().Validate(new GrantServiceClientTenantCommand(Guid.Empty, TenantId)).IsValid.Should().BeFalse();
        new GrantServiceClientTenantCommandValidator().Validate(new GrantServiceClientTenantCommand(ClientId, Guid.Empty)).IsValid.Should().BeFalse();
        new RevokeServiceClientTenantCommandValidator().Validate(new RevokeServiceClientTenantCommand(ClientId, TenantId)).IsValid.Should().BeTrue();
        new RevokeServiceClientTenantCommandValidator().Validate(new RevokeServiceClientTenantCommand(ClientId, Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void RevokeSecretValidator_RequiresBothIdentifiers()
    {
        var validator = new RevokeServiceClientSecretCommandValidator();
        validator.Validate(new RevokeServiceClientSecretCommand(ClientId, TenantId)).IsValid.Should().BeTrue();
        validator.Validate(new RevokeServiceClientSecretCommand(Guid.Empty, TenantId)).IsValid.Should().BeFalse();
        validator.Validate(new RevokeServiceClientSecretCommand(ClientId, Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetSecretsValidator_RequiresClientId()
    {
        var validator = new GetServiceClientSecretsQueryValidator();
        validator.Validate(new GetServiceClientSecretsQuery(ClientId)).IsValid.Should().BeTrue();
        validator.Validate(new GetServiceClientSecretsQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }
}
