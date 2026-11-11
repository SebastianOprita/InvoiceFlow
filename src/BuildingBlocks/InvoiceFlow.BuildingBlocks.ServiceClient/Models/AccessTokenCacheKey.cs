namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public readonly record struct AccessTokenCacheKey(
    string ClientId,
    Guid TenantId);
