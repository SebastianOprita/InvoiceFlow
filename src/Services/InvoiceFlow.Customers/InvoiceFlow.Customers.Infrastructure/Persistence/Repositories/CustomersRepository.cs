using InvoiceFlow.Customers.Application;
using InvoiceFlow.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Customers.Infrastructure;

public class CustomersRepository(CustomersDbContext dbContext) : ICustomersRepository
{
    public async Task<List<Customer>> FindAllCustomersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> FindCustomerByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId,
            cancellationToken);
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId,
            cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, CustomerCode customerCode, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.CustomerCode == customerCode, cancellationToken);
    }

    public async Task<bool> ExistsByRegistrationNumberAsync(Guid tenantId, RegistrationNumber registrationNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.RegistrationNumber == registrationNumber, cancellationToken);
    }

    public async Task<bool> ExistsByTaxNumberAsync(Guid tenantId, TaxNumber taxNumber, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.TaxNumber == taxNumber, cancellationToken);
    }

    public void AddCustomer(Customer customer)
    {
        dbContext.Customers.Add(customer);
    }

    public void RemoveCustomer(Customer customer)
    {
        dbContext.Customers.Remove(customer);
    }
}
