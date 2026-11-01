using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IServiceClientsRepository
{
    Task<ServiceClient?> GetServiceClientByIdAsync(
        Guid serviceClientId,
        CancellationToken cancellationToken);

    Task<ServiceClient?> GetTrackedServiceClientByIdAsync(
        Guid serviceClientId,
        CancellationToken cancellationToken);
}
