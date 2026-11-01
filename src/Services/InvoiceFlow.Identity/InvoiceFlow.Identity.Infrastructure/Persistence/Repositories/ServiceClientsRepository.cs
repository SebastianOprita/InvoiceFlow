using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

internal sealed class ServiceClientsRepository(
    IdentityDbContext dbContext)
    : IServiceClientsRepository
{
    public async Task<ServiceClient?> GetServiceClientByIdAsync(Guid serviceClientId, CancellationToken cancellationToken)
    {
        return await dbContext.ServiceClients
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Secrets)
            .Include(x => x.Tenants)
            .FirstOrDefaultAsync(
                x => x.Id == serviceClientId,
                cancellationToken);
    }

    public async Task<ServiceClient?> GetTrackedServiceClientByIdAsync(Guid serviceClientId, CancellationToken cancellationToken)
    {
        return await dbContext.ServiceClients
            .AsSplitQuery()
            .Include(x => x.Secrets)
            .Include(x => x.Tenants)
            .FirstOrDefaultAsync(
                x => x.Id == serviceClientId,
                cancellationToken);
    }
}
