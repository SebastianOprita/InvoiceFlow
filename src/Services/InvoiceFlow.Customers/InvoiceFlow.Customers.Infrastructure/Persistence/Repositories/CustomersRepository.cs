using InvoiceFlow.Customers.Application;
using InvoiceFlow.Customers.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Customers.Infrastructure;

public class CustomersRepository(CustomersDbContext dbContext) : ICustomersRepository
{
    public List<Customer> FindAllCustomers(Guid tenantId)
    {
        return dbContext.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToList();
    }

    public Customer? FindCustomerById(Guid tenantId, Guid customerId)
    {
        return dbContext.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Id == customerId)
            .FirstOrDefault();
    }
}
