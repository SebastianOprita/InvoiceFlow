namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public sealed class ServiceTokenOptions
{
    public const string SectionName = "ServiceClient";

    public required string TokenEndpoint { get; init; }

    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }
    public required string GrantType { get; init; } = "client_credentials";

    public TimeSpan RefreshSkew { get; init; } =
        TimeSpan.FromSeconds(30);

    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromSeconds(10);
}
