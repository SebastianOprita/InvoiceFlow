using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.ClientSecrets;

public sealed class ServiceClientSecretHandlersTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateSecret_ReturnsNotFoundWhenClientIsMissing()
    {
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceClient?)null);
        var sut = new CreateServiceClientSecretCommandHandler(clients.Object, Mock.Of<IOrganizationsRepository>(), Mock.Of<IServiceClientSecretService>(), Mock.Of<IUnitOfWork>(), Clock());

        var result = await sut.Handle(new(Guid.NewGuid(), "key", null, Now.AddDays(1)), TestContext.Current.CancellationToken);

        result.Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
    }

    [Fact]
    public async Task CreateSecret_StoresHashAndReturnsGeneratedSecret()
    {
        var (client, org) = CreateClient();
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var organizations = new Mock<IOrganizationsRepository>();
        organizations.Setup(x => x.GetOrganizationByIdAsync(org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        var secretService = new Mock<IServiceClientSecretService>();
        var hash = ServiceClientSecretHash.Create(new string('A', 64));
        secretService.Setup(x => x.GenerateSecretAndHash()).Returns(("one-time-secret", hash));
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var sut = new CreateServiceClientSecretCommandHandler(clients.Object, organizations.Object, secretService.Object, unit.Object, Clock());

        var result = await sut.Handle(new(client.Id, "  deploy  ", "  CI key ", Now.AddDays(5)), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Secret.Should().Be("one-time-secret");
        result.Value.Name.Should().Be("deploy");
        client.Secrets.Single().SecretHash.Should().Be(hash);
        client.Secrets.Single().Description!.Value.Should().Be("CI key");
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSecret_ReturnsOrganizationMissingAndInactiveFailures()
    {
        var (client, org) = CreateClient();
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var organizations = new Mock<IOrganizationsRepository>();
        var sut = new CreateServiceClientSecretCommandHandler(clients.Object, organizations.Object, Mock.Of<IServiceClientSecretService>(), Mock.Of<IUnitOfWork>(), Clock());
        var command = new CreateServiceClientSecretCommand(client.Id, "key", null, Now.AddDays(1));

        (await sut.Handle(command, TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.OrganizationNotFound);
        org.Deactivate(Now.AddMinutes(1));
        organizations.Setup(x => x.GetOrganizationByIdAsync(org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        (await sut.Handle(command, TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.OrganizationInactive);
    }

    [Fact]
    public async Task CreateSecret_ReturnsClientInactiveWithoutGeneratingSecret()
    {
        var (client, org) = CreateClient();
        client.Deactivate(Now.AddMinutes(1));
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var organizations = new Mock<IOrganizationsRepository>();
        organizations.Setup(x => x.GetOrganizationByIdAsync(org.Id, It.IsAny<CancellationToken>())).ReturnsAsync(org);
        var secrets = new Mock<IServiceClientSecretService>();
        var unit = new Mock<IUnitOfWork>();
        var sut = new CreateServiceClientSecretCommandHandler(clients.Object, organizations.Object, secrets.Object, unit.Object, Clock());

        var result = await sut.Handle(new(client.Id, "key", null, Now.AddDays(1)), TestContext.Current.CancellationToken);

        result.Error.Should().Be(ApplicationErrors.ServiceClientInactive);
        secrets.Verify(x => x.GenerateSecretAndHash(), Times.Never);
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSecret_RevokesAndSavesOrReturnsNotFound()
    {
        var (client, _) = CreateClient();
        var secret = client.AddSecret(Guid.NewGuid(), ServiceClientSecretHash.Create(new string('B', 64)), ServiceClientSecretName.Create("key"), null, Now.AddDays(1), Now);
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var sut = new RevokeServiceClientSecretCommandHandler(clients.Object, unit.Object, Clock());

        (await sut.Handle(new(client.Id, secret.Id), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        secret.RevokedAtUtc.Should().Be(Now);
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceClient?)null);
        (await sut.Handle(new(client.Id, secret.Id), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
    }

    [Fact]
    public async Task RevokeSecret_ReturnsUnitOfWorkFailure()
    {
        var (client, _) = CreateClient();
        var secret = client.AddSecret(Guid.NewGuid(), ServiceClientSecretHash.Create(new string('B', 64)), ServiceClientSecretName.Create("key"), null, Now.AddDays(1), Now);
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure(ApplicationErrors.ConcurrencyConflict));
        var sut = new RevokeServiceClientSecretCommandHandler(clients.Object, unit.Object, Clock());

        var result = await sut.Handle(new(client.Id, secret.Id), TestContext.Current.CancellationToken);

        result.Error.Should().Be(ApplicationErrors.ConcurrencyConflict);
        secret.RevokedAtUtc.Should().Be(Now);
    }

    [Fact]
    public async Task GrantAndRevokePermissions_UpdateClientAndSave()
    {
        var (client, _) = CreateClient();
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var clock = Clock();
        var grant = new GrantServiceClientPermissionsCommandHandler(clients.Object, unit.Object, clock);
        var revoke = new RevokeServiceClientPermissionsCommandHandler(clients.Object, unit.Object, clock);
        const long permission = 1L << 3;

        (await grant.Handle(new(client.Id, permission), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        client.HasPermission(permission).Should().BeTrue();
        (await revoke.Handle(new(client.Id, permission), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        client.HasPermission(permission).Should().BeFalse();
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task PermissionHandlers_ReturnNotFoundWhenClientIsMissing()
    {
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceClient?)null);
        var unit = new Mock<IUnitOfWork>();
        var grant = new GrantServiceClientPermissionsCommandHandler(clients.Object, unit.Object, Clock());
        var revoke = new RevokeServiceClientPermissionsCommandHandler(clients.Object, unit.Object, Clock());
        var id = Guid.NewGuid();

        (await grant.Handle(new(id, 1L), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
        (await revoke.Handle(new(id, 1L), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GrantAndRevokeTenant_UpdateClientAndHandleMissingResources()
    {
        var (client, _) = CreateClient();
        var tenantId = Guid.NewGuid();
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var tenants = new Mock<ITenantsRepository>();
        var tenant = Tenant.Create(tenantId, TenantName.Create("Tenant"), TenantSlug.Create("tenant"), Now);
        tenants.Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        var unit = new Mock<IUnitOfWork>();
        unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var clock = Clock();
        var grant = new GrantServiceClientTenantCommandHandler(clients.Object, tenants.Object, unit.Object, clock);
        var revoke = new RevokeServiceClientTenantCommandHandler(clients.Object, unit.Object, clock);

        (await grant.Handle(new(client.Id, tenantId), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        client.CanAccessTenant(tenantId).Should().BeTrue();
        (await revoke.Handle(new(client.Id, tenantId), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        client.CanAccessTenant(tenantId).Should().BeFalse();
        tenants.Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);
        (await grant.Handle(new(client.Id, tenantId), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.TenantNotFound);
    }

    [Fact]
    public async Task TenantHandlers_ReturnNotFoundWhenClientIsMissing()
    {
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetTrackedServiceClientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceClient?)null);
        var tenants = new Mock<ITenantsRepository>();
        var unit = new Mock<IUnitOfWork>();
        var grant = new GrantServiceClientTenantCommandHandler(clients.Object, tenants.Object, unit.Object, Clock());
        var revoke = new RevokeServiceClientTenantCommandHandler(clients.Object, unit.Object, Clock());
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        (await grant.Handle(new(id, tenantId), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
        (await revoke.Handle(new(id, tenantId), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
        tenants.Verify(x => x.GetTenantByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateServiceClientToken_ReturnsTokenForValidClientSecretAndTenant()
    {
        var (client, organization) = CreateClient();
        var tenantId = Guid.NewGuid();
        client.GrantTenant(tenantId, Now);
        var hash = ServiceClientSecretHash.Create(new string('D', 64));
        client.AddSecret(Guid.NewGuid(), hash, ServiceClientSecretName.Create("token"), null, Now.AddDays(1), Now);
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetServiceClientByClientIdAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var organizations = new Mock<IOrganizationsRepository>();
        organizations.Setup(x => x.GetOrganizationByIdAsync(organization.Id, It.IsAny<CancellationToken>())).ReturnsAsync(organization);
        var tenants = new Mock<ITenantsRepository>();
        var tenant = Tenant.Create(tenantId, TenantName.Create("Tenant"), TenantSlug.Create("tenant"), Now);
        tenants.Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        var secrets = new Mock<IServiceClientSecretService>();
        secrets.Setup(x => x.Verify("clear-secret", hash)).Returns(true);
        var tokens = new Mock<ITokenService>();
        var expiresAt = Now.AddHours(1);
        tokens.Setup(x => x.GenerateServiceClientToken(client, tenantId, Now)).Returns(("access-token", "Bearer", expiresAt));
        var sut = new GenerateServiceClientTokenCommandHandler(clients.Object, organizations.Object, tenants.Object, secrets.Object, tokens.Object, Clock());

        var result = await sut.Handle(new(tenantId, client.ClientId.Value, "clear-secret"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.AccessTokenExpiresAt.Should().Be(expiresAt);
        tokens.Verify(x => x.GenerateServiceClientToken(client, tenantId, Now), Times.Once);
    }

    [Fact]
    public async Task GenerateServiceClientToken_RejectsInvalidCredentialsAndTenantAccess()
    {
        var (client, organization) = CreateClient();
        var tenantId = Guid.NewGuid();
        var hash = ServiceClientSecretHash.Create(new string('E', 64));
        client.AddSecret(Guid.NewGuid(), hash, ServiceClientSecretName.Create("token"), null, Now.AddDays(1), Now);
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetServiceClientByClientIdAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var organizations = new Mock<IOrganizationsRepository>();
        organizations.Setup(x => x.GetOrganizationByIdAsync(organization.Id, It.IsAny<CancellationToken>())).ReturnsAsync(organization);
        var tenants = new Mock<ITenantsRepository>();
        var tenant = Tenant.Create(tenantId, TenantName.Create("Tenant"), TenantSlug.Create("tenant"), Now);
        tenants.Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        var secrets = new Mock<IServiceClientSecretService>();
        secrets.Setup(x => x.Verify("bad-secret", hash)).Returns(false);
        var tokens = new Mock<ITokenService>();
        var sut = new GenerateServiceClientTokenCommandHandler(clients.Object, organizations.Object, tenants.Object, secrets.Object, tokens.Object, Clock());

        (await sut.Handle(new(tenantId, client.ClientId.Value, "bad-secret"), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.InvalidServiceClientCredentials);
        secrets.Setup(x => x.Verify("clear-secret", hash)).Returns(true);
        (await sut.Handle(new(tenantId, client.ClientId.Value, "clear-secret"), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientAccessDenied);
        tenants.Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);
        client.GrantTenant(tenantId, Now);
        (await sut.Handle(new(tenantId, client.ClientId.Value, "clear-secret"), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientAccessDenied);
        tokens.Verify(x => x.GenerateServiceClientToken(It.IsAny<ServiceClient>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    [Fact]
    public async Task GetSecrets_ReturnsChronologicalDtosOrNotFound()
    {
        var (client, _) = CreateClient();
        var hash = ServiceClientSecretHash.Create(new string('C', 64));
        client.AddSecret(Guid.NewGuid(), hash, ServiceClientSecretName.Create("later"), null, Now.AddDays(10), Now.AddDays(1));
        client.AddSecret(Guid.NewGuid(), hash, ServiceClientSecretName.Create("earlier"), ServiceClientSecretDescription.Create("description"), Now.AddDays(10), Now);
        var clients = new Mock<IServiceClientsRepository>();
        clients.Setup(x => x.GetServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        var sut = new GetServiceClientSecretsQueryHandler(clients.Object);

        var result = await sut.Handle(new(client.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Select(x => x.Name).Should().Equal("earlier", "later");
        result.Value[0].Description.Should().Be("description");
        clients.Setup(x => x.GetServiceClientByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceClient?)null);
        (await sut.Handle(new(client.Id), TestContext.Current.CancellationToken)).Error.Should().Be(ApplicationErrors.ServiceClientNotFound);
    }

    private static ISystemDateTimeProvider Clock()
    {
        var clock = new Mock<ISystemDateTimeProvider>();
        clock.Setup(x => x.GetUtcNow()).Returns(Now);
        return clock.Object;
    }

    private static (ServiceClient Client, Organization Organization) CreateClient()
    {
        var organization = Organization.Create(Guid.NewGuid(), OrganizationName.Create("Org"), OrganizationTypes.Partner, Now);
        var client = ServiceClient.Create(Guid.NewGuid(), organization, ServiceClientIdentifier.Create("service-client"), ServiceClientName.Create("Test"), Now);
        return (client, organization);
    }
}
