namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public interface IAccessTokenProvider
{
    Task<AccessToken> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
