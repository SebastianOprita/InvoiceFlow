using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

internal sealed class ServiceClientsRepository(
    IdentityDbContext dbContext)
    : IServiceClientsRepository
{
<<<<<<< Updated upstream
    public async Task<ServiceClient?> GetServiceClientByIdAsync(Guid serviceClientId, CancellationToken cancellationToken)
=======
    public async Task<ServiceClient?> GetServiceClientByClientIdAsync(ServiceClientId serviceClientId, CancellationToken cancellationToken)
>>>>>>> Stashed changes
    {
        return await dbContext.ServiceClients
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Secrets)
            .Include(x => x.Tenants)
            .FirstOrDefaultAsync(
<<<<<<< Updated upstream
                x => x.Id == serviceClientId,
                cancellationToken);
    }

    public async Task<ServiceClient?> GetTrackedServiceClientByIdAsync(Guid serviceClientId, CancellationToken cancellationToken)
=======
                x => x.ClientId == serviceClientId,
                cancellationToken);
    }

    public async Task<ServiceClient?> GetTrackedServiceClientByClientIdAsync(ServiceClientId serviceClientId, CancellationToken cancellationToken)
>>>>>>> Stashed changes
    {
        return await dbContext.ServiceClients
            .AsSplitQuery()
            .Include(x => x.Secrets)
            .Include(x => x.Tenants)
            .FirstOrDefaultAsync(
<<<<<<< Updated upstream
                x => x.Id == serviceClientId,
=======
                x => x.ClientId == serviceClientId,
>>>>>>> Stashed changes
                cancellationToken);
    }
}
