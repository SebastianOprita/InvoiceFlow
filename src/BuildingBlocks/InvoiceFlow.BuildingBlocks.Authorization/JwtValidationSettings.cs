namespace InvoiceFlow.BuildingBlocks.Authorization;

public sealed class JwtValidationSettings
{
    public string Issuer { get; set; } = "InvoiceFlow.Identity";
    public string Audience { get; set; } = "InvoiceFlow.Api";
    public string Secret { get; set; } = "";
}
