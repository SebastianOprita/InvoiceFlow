using System.Text.Json.Serialization;

namespace InvoiceFlow.Identity.Api;

public record CreateTenantRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Slug);
public record UpdateTenantRequest(
    [property: JsonRequired] string Name);
