using System.Text.Json.Serialization;

namespace InvoiceFlow.Identity.Api;

public sealed record CreateServiceClientSecretRequest(
    [property: JsonRequired] string Name,
    string? Description,
    [property: JsonRequired] DateTimeOffset ExpiresAtUtc);

public sealed record GrantServiceClientPermissionsRequest(
    [property: JsonRequired] long Permission);

public sealed record GrantServiceClientTenantRequest(
    [property: JsonRequired] Guid TenantId);
