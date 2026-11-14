namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public interface ITokenEndpointClient
{
    Task<AccessToken> RequestAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}
