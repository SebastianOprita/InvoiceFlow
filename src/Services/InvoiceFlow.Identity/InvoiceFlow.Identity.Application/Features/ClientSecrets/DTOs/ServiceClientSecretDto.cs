namespace InvoiceFlow.Identity.Application;

public sealed record ServiceClientSecretDto(
    Guid Id,
    Guid ServiceClientId,
    string Name,
    string? Description,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc);
