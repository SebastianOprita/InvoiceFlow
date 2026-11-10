namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public sealed record AccessToken(
    string Value,
    DateTimeOffset ExpiresAtUtc);
