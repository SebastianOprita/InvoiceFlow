using InvoiceFlow.Customers.Domain;

namespace InvoiceFlow.Customers.Application;

public interface ICustomersRepository
{
    public List<Customer> FindAllCustomers(Guid tenantId);
    public Customer? FindCustomerById(Guid tenantId, Guid customerId);
    public Customer? GetCustomerById(Guid tenantId, Guid customerId);
    public bool ExistsByCode(Guid tenantId, CustomerCode customerCode);
    public bool ExistsByRegistrationNumber(Guid tenantId, RegistrationNumber registrationNumber);
    public bool ExistsByTaxNumber(Guid tenantId, TaxNumber taxNumber);
    public void AddCustomer(Customer customer);
    public void RemoveCustomer(Customer customer);
}
