namespace InvoiceFlow.Identity.Application;

public sealed record CreateServiceClientSecretCommandResponse(
    Guid Id,
    Guid ServiceClientId,
    string Name,
    string Secret,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
