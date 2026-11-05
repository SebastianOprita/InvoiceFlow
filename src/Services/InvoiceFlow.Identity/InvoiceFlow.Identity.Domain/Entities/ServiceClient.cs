using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class ServiceClient
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public ServiceClientIdentifier ClientId { get; private set; }
    public ServiceClientName Name { get; private set; }
    public bool IsActive { get; private set; }
    public bool CanAccessAllTenants { get; private set; }
    public Permissions Permissions { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public const int MaxActiveSecrets = 2;

    // Navigation property
    private readonly List<ServiceClientSecret> _secrets = [];
    public IReadOnlyCollection<ServiceClientSecret> Secrets => _secrets;

    private readonly List<ServiceClientTenant> _tenants = [];
    public IReadOnlyCollection<ServiceClientTenant> Tenants => _tenants;

#pragma warning disable CS8618
    private ServiceClient() { } // EF Core
#pragma warning restore CS8618

    private ServiceClient(
        Guid id,
        Guid organizationId,
        ServiceClientIdentifier clientId,
        ServiceClientName name,
        bool canAccessAllTenants,
        DateTimeOffset createdAtUtc,
        Permissions? permissions = null)
    {
        Id = id;
        OrganizationId = organizationId;
        ClientId = clientId;
        Name = name;
        CanAccessAllTenants = canAccessAllTenants;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        Permissions = permissions ?? Permissions.None;
    }

    public static ServiceClient Create(
        Guid id,
        Organization organization,
        ServiceClientIdentifier clientId,
        ServiceClientName name,
        DateTimeOffset createdAtUtc,
        Permissions? permissions = null)
    {
        if (organization is null || organization.Id == Guid.Empty)
            throw new DomainException(DomainErrors.OrganizationIdRequired);

        if (!organization.IsActive)
            throw new DomainException(DomainErrors.OrganizationInactive);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.ServiceClientIdRequired);

        if (clientId is null)
            throw new DomainException(DomainErrors.ServiceClientIdRequired);

        if (name is null)
            throw new DomainException(DomainErrors.ServiceClientNameRequired);

        return new ServiceClient(
            id,
            organization.Id,
            clientId,
            name,
            organization.Type == OrganizationTypes.Internal,
            createdAtUtc.ToUniversalTime(),
            permissions);
    }

    public void Rename(
        ServiceClientName name,
        DateTimeOffset updatedAtUtc)
    {
        updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(updatedAtUtc);

        if (name is null)
            throw new DomainException(DomainErrors.ServiceClientNameInvalid);

        if (Name == name)
            return;

        Name = name;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(updatedAtUtc);

        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(updatedAtUtc);

        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    public ServiceClientSecret AddSecret(
        Guid secretId,
        ServiceClientSecretHash secretHash,
        ServiceClientSecretName secretName,
        ServiceClientSecretDescription? secretDescription,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc)
    {
        var updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(createdAtUtc);
        expiresAtUtc = expiresAtUtc.ToUniversalTime();

        if (!IsActive)
            throw new DomainException(DomainErrors.ServiceClientInactive);

        if (_secrets.Count(x => x.IsActive(updatedAtUtc)) >= MaxActiveSecrets)
            throw new DomainException(DomainErrors.ServiceClientSecretLimitReached);

        if (_secrets.Any(x => x.Id == secretId))
            throw new DomainException(DomainErrors.ServiceClientSecretAlreadyExists);

        var secret = ServiceClientSecret.Create(
            secretId,
            Id,
            secretHash,
            secretName,
            secretDescription,
            updatedAtUtc,
            expiresAtUtc);

        _secrets.Add(secret);

        UpdatedAtUtc = updatedAtUtc;

        return secret;
    }

    public void RevokeSecret(
        Guid secretId,
        DateTimeOffset revokedAtUtc)
    {
        var updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(revokedAtUtc);

        var secret = _secrets.FirstOrDefault(x => x.Id == secretId);

        if (secret is null)
            throw new DomainException(DomainErrors.ServiceClientSecretNotFound);

        if (secret.RevokedAtUtc is not null)
            return;

        secret.Revoke(updatedAtUtc);

        UpdatedAtUtc = updatedAtUtc;
    }

    public void GrantPermission(long permission, DateTimeOffset updatedAtUtc)
    {
        updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(updatedAtUtc);

        var newPermissions = Permissions.Grant(permission);
        if (newPermissions == Permissions)
            return;

        Permissions = newPermissions;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void RevokePermission(long permission, DateTimeOffset updatedAtUtc)
    {
        updatedAtUtc = ValidateAndNormalizeUpdatedAtUtc(updatedAtUtc);

        var newPermissions = Permissions.Revoke(permission);
        if (Permissions == newPermissions)
            return;

        Permissions = newPermissions;
        UpdatedAtUtc = updatedAtUtc;
    }

    public bool HasPermission(long permission)
    {
        return Permissions.Has(permission);
    }

    public void GrantTenant(
        Guid tenantId,
        DateTimeOffset now)
    {
        now = ValidateAndNormalizeUpdatedAtUtc(now);

        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (CanAccessAllTenants)
            return;

        if (_tenants.Any(x => x.TenantId == tenantId))
            return;

        _tenants.Add(ServiceClientTenant.Create(
            Id,
            tenantId,
            now));

        UpdatedAtUtc = now;
    }

    public void RevokeTenant(
        Guid tenantId,
        DateTimeOffset now)
    {
        now = ValidateAndNormalizeUpdatedAtUtc(now);

        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (CanAccessAllTenants)
            return;

        var existing = _tenants.FirstOrDefault(
            x => x.TenantId == tenantId);

        if (existing is null)
            return;

        _tenants.Remove(existing);

        UpdatedAtUtc = now;
    }

    public bool CanAccessTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return false;

        return CanAccessAllTenants ||
            _tenants.Any(x => x.TenantId == tenantId);
    }

    private DateTimeOffset ValidateAndNormalizeUpdatedAtUtc(DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        updatedAtUtc = updatedAtUtc.ToUniversalTime();

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        return updatedAtUtc;
    }
}
