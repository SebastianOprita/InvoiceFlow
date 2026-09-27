using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Domain;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class CreateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    ICustomersRepository customersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand cmd, CancellationToken cancellationToken)
    {
        if (await customersRepository.ExistsByCodeAsync(cmd.TenantId, CustomerCode.Create(cmd.CustomerCode), cancellationToken))
            return Result<CustomerDto>.Failure(ApplicationErrors.CreateCustomerCodeAlreadyExists);

        if (await customersRepository.ExistsByRegistrationNumberAsync(cmd.TenantId, RegistrationNumber.Create(cmd.CustomerTaxDetails.RegistrationNumber), cancellationToken))
            return Result<CustomerDto>.Failure(ApplicationErrors.CreateCustomerRegistrationNumberAlreadyExists);

        if (cmd.CustomerTaxDetails.TaxNumber is not null 
            && await customersRepository.ExistsByTaxNumberAsync(cmd.TenantId, TaxNumber.Create(cmd.CustomerTaxDetails.TaxNumber), cancellationToken))
                return Result<CustomerDto>.Failure(ApplicationErrors.CreateCustomerTaxNumberAlreadyExists);

        var customer = Customer.Create(
            Guid.CreateVersion7(),
            cmd.TenantId,
            CustomerCode.Create(cmd.CustomerCode),
            CustomerName.Create(cmd.Name),
            CustomerEmail.CreateOptional(cmd.CustomerContact.Email),
            PhoneNumber.CreateOptional(cmd.CustomerContact.Phone),
            TaxNumber.CreateOptional(cmd.CustomerTaxDetails.TaxNumber),
            RegistrationNumber.Create(cmd.CustomerTaxDetails.RegistrationNumber),
            AddressLine1.Create(cmd.CustomerAddress.AddressLine1),
            AddressLine2.CreateOptional(cmd.CustomerAddress.AddressLine2),
            City.Create(cmd.CustomerAddress.City),
            State.CreateOptional(cmd.CustomerAddress.State),
            Country.Create(cmd.CustomerAddress.Country),
            PostalCode.Create(cmd.CustomerAddress.PostalCode),
            CurrencyCode.Create(cmd.CurrencyCode),
            CreditLimit.Create(cmd.CustomerCreditPolicy.CreditLimit),
            PaymentTermDays.Create(cmd.CustomerCreditPolicy.PaymentTermDays),
            dateTimeProvider.Now);

        customersRepository.AddCustomer(customer);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<CustomerDto>.Failure(result);

        return Result<CustomerDto>.Success(customer.ToDto());
    }
}
