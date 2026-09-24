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

    public Customer? GetCustomerById(Guid tenantId, Guid customerId)
    {
        return dbContext.Customers
            .Where(c => c.TenantId == tenantId && c.Id == customerId)
            .FirstOrDefault();
    }

    public bool ExistsByCode(Guid tenantId, CustomerCode customerCode)
    {
        return dbContext.Customers.AsNoTracking()
            .Any(x => x.TenantId == tenantId && x.CustomerCode == customerCode);
    }

    public bool ExistsByRegistrationNumber(Guid tenantId, RegistrationNumber registrationNumber)
    {
        return dbContext.Customers.AsNoTracking()
            .Any(x => x.TenantId == tenantId && x.RegistrationNumber == registrationNumber);
    }

    public bool ExistsByTaxNumber(Guid tenantId, TaxNumber taxNumber)
    {
        return dbContext.Customers.AsNoTracking()
            .Any(x => x.TenantId == tenantId && x.TaxNumber == taxNumber);
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
