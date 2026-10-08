using InvoiceFlow.Customers.Domain;

namespace InvoiceFlow.Customers.Application;

public interface ICustomersRepository
{
    public Task<List<Customer>> GetAllCustomersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    public Task<Customer?> GetCustomerByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    public Task<Customer?> GetTrackedCustomerByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByCodeAsync(Guid tenantId, CustomerCode customerCode, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByRegistrationNumberAsync(Guid tenantId, RegistrationNumber registrationNumber, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByTaxNumberAsync(Guid tenantId, TaxNumber taxNumber, CancellationToken cancellationToken = default);
    public void AddCustomer(Customer customer);
    public void RemoveCustomer(Customer customer);
}
