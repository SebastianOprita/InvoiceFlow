namespace InvoiceFlow.BuildingBlocks.Authorization;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "InvoiceFlow.Identity";
    public string Audience { get; set; } = "InvoiceFlow.Api";
    public string Secret { get; set; } = "f8K2lP9xQw3Zr7TnV6bY1uI4oA0sD5FgH";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
    public int ImpersonationTokenMinutes { get; set; } = 15;
}
