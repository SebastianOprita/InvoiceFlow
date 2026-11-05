using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class ServiceClientSecret
{
    public Guid Id { get; private set; }
    public Guid ServiceClientId { get; private set; }
    public ServiceClientSecretHash SecretHash { get; private set; }
    public ServiceClientSecretName Name { get; private set; }
    public ServiceClientSecretDescription? Description { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }

#pragma warning disable CS8618
    private ServiceClientSecret() { } // EF Core
#pragma warning restore CS8618

    private ServiceClientSecret(
        Guid id,
        Guid serviceClientId,
        ServiceClientSecretHash secretHash,
        ServiceClientSecretName name,
        ServiceClientSecretDescription? description,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        Id = id;
        ServiceClientId = serviceClientId;
        SecretHash = secretHash;
        Name = name;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    internal static ServiceClientSecret Create(
        Guid id,
        Guid serviceClientId,
        ServiceClientSecretHash secretHash,
        ServiceClientSecretName name,
        ServiceClientSecretDescription? description,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (serviceClientId == Guid.Empty)
            throw new DomainException(DomainErrors.ServiceClientIdInvalid);

        if (secretHash is null)
            throw new DomainException(DomainErrors.ServiceClientSecretHashRequired);

        if (name is null)
            throw new DomainException(DomainErrors.ServiceClientSecretNameRequired);

        createdAtUtc = createdAtUtc.ToUniversalTime();
        expiresAtUtc = expiresAtUtc.ToUniversalTime();

        if (expiresAtUtc <= createdAtUtc)
            throw new DomainException(DomainErrors.ExpiresAtUtcInvalid);

        return new ServiceClientSecret(
            id,
            serviceClientId,
            secretHash,
            name,
            description,
            createdAtUtc,
            expiresAtUtc);
    }

    public bool IsActive(DateTimeOffset now) =>
        RevokedAtUtc is null && ExpiresAtUtc > now.ToUniversalTime();

    internal void Revoke(DateTimeOffset revokedAtUtc)
    {
        revokedAtUtc = revokedAtUtc.ToUniversalTime();

        if (RevokedAtUtc is not null)
            return;

        if (revokedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.RevokedAtUtcInvalid);

        RevokedAtUtc = revokedAtUtc;
    }
}
