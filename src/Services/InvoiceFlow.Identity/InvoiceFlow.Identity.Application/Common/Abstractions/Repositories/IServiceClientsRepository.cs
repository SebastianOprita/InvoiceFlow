using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IServiceClientsRepository
{
<<<<<<< Updated upstream
    Task<ServiceClient?> GetServiceClientByIdAsync(
        Guid serviceClientId,
        CancellationToken cancellationToken);

    Task<ServiceClient?> GetTrackedServiceClientByIdAsync(
        Guid serviceClientId,
=======
    Task<ServiceClient?> GetServiceClientByClientIdAsync(
        ServiceClientId serviceClientId,
        CancellationToken cancellationToken);

    Task<ServiceClient?> GetTrackedServiceClientByClientIdAsync(
        ServiceClientId serviceClientId,
>>>>>>> Stashed changes
        CancellationToken cancellationToken);
}
