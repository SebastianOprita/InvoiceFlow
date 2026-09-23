using InvoiceFlow.Customers.Domain;

namespace InvoiceFlow.Customers.Application;

public interface ICustomersRepository
{
    public List<Customer> FindAllCustomers(Guid tenantId);
    public Customer? FindCustomerById(Guid tenantId, Guid customerId);
}
