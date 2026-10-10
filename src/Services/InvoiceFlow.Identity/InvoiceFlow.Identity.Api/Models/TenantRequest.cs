namespace InvoiceFlow.Identity.Api;

public record CreateTenantRequest(string Name, string Slug);
public record UpdateTenantRequest(string Name);
