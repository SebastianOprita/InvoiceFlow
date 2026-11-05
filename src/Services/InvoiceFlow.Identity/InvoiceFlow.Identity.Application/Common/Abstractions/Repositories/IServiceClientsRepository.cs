using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IServiceClientsRepository
{
    Task<ServiceClient?> GetServiceClientByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ServiceClient?>  GetTrackedServiceClientByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ServiceClient?> GetServiceClientByClientIdAsync(
        ServiceClientIdentifier serviceClientId,
        CancellationToken cancellationToken);

    Task<ServiceClient?> GetTrackedServiceClientByClientIdAsync(
        ServiceClientIdentifier serviceClientId,
        CancellationToken cancellationToken);
}
