using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public static class ServiceClientSecretMapper
{
    public static ServiceClientSecretDto ToDto(this ServiceClientSecret secret)
    {
        return new ServiceClientSecretDto
        (
            Id: secret.Id,
            ServiceClientId: secret.ServiceClientId,
            Name: secret.Name.Value,
            Description: secret.Description?.Value,
            CreatedAtUtc: secret.CreatedAtUtc,
            ExpiresAtUtc: secret.ExpiresAtUtc,
            RevokedAtUtc: secret.RevokedAtUtc
        );
    }
}
