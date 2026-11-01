using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Identity.Infrastructure;

internal sealed class OrganizationsRepository(
    IdentityDbContext dbContext)
    : IOrganizationsRepository
{
    public async Task<Organization?> GetOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return await dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == organizationId,
                cancellationToken);
    }
}
