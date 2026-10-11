using System.Text.Json.Serialization;

namespace InvoiceFlow.Identity.Api;

public record CreatePlatformUserRequest(
    [property: JsonRequired] string Email,
    [property: JsonRequired] string Password,
    [property: JsonRequired] string FirstName,
    [property: JsonRequired] string LastName);
public record UpdatePlatformUserRequest(
    [property: JsonRequired] string FirstName,
    [property: JsonRequired] string LastName);
public record ImpersonateUserRequest(
    [property: JsonRequired] Guid TargetUserTenantId,
    [property: JsonRequired] string TargetUserEmail,
    [property: JsonRequired] string Reason);
