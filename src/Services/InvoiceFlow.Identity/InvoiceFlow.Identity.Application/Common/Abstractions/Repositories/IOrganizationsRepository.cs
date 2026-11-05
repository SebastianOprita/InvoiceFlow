using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IOrganizationsRepository
{
    Task<Organization?> GetOrganizationByIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken);
}
