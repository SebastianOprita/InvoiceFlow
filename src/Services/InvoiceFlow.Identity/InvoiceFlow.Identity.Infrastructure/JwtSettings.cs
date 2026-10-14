namespace InvoiceFlow.Identity.Infrastructure;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "InvoiceFlow.Identity";
    public string Audience { get; set; } = "InvoiceFlow.Api";
    public string Secret { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
    public int ImpersonationTokenMinutes { get; set; } = 15;
}
