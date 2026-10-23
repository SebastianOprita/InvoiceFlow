using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Domain;
using MediatR;

namespace InvoiceFlow.Customers.Application;

internal class UpdateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    ICustomersRepository customersRepository,
    ISystemDateTimeProvider dateTimeProvider) 
    : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand cmd, CancellationToken cancellationToken)
    {
        var customer = await customersRepository.GetTrackedCustomerByIdAsync(cmd.TenantId, cmd.CustomerId, cancellationToken);
        if (customer is null)
            return Result<CustomerDto>.Failure(ApplicationErrors.CustomerNotFound);

        var now = dateTimeProvider.GetUtcNow();

        if (cmd.Name is not null)
            customer.UpdateName(CustomerName.Create(cmd.Name), now);

        if (cmd.CustomerContact is not null)
            customer.UpdateContact(
                CustomerEmail.CreateOptional(cmd.CustomerContact.Email), 
                PhoneNumber.CreateOptional(cmd.CustomerContact.Phone),
                now);

        if (cmd.CustomerAddress is not null)
            customer.UpdateAddress(
                AddressLine1.Create(cmd.CustomerAddress.AddressLine1), 
                AddressLine2.CreateOptional(cmd.CustomerAddress.AddressLine2), 
                City.Create(cmd.CustomerAddress.City), 
                State.CreateOptional(cmd.CustomerAddress.State), 
                Country.Create(cmd.CustomerAddress.Country), 
                PostalCode.Create(cmd.CustomerAddress.PostalCode),
                now);

        if (cmd.CustomerCreditPolicy is not null)
            customer.UpdateCreditPolicy(
                CreditLimit.Create(cmd.CustomerCreditPolicy.CreditLimit), 
                PaymentTermDays.Create(cmd.CustomerCreditPolicy.PaymentTermDays),
                now);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (!result.IsSuccess)
            return Result<CustomerDto>.Failure(result.Error!);

        return Result<CustomerDto>.Success(customer.ToDto());
    }
}
