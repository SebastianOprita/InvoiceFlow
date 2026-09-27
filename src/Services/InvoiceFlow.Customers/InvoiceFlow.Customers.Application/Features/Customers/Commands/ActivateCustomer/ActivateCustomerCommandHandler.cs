using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class ActivateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    ICustomersRepository customersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ActivateCustomerCommand, Result>
{
    public async Task<Result> Handle(ActivateCustomerCommand cmd, CancellationToken cancellationToken)
    {
        var customer = await customersRepository.GetCustomerByIdAsync(cmd.TenantId, cmd.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure(ApplicationErrors.ActivateCustomerNotFound);

        if (customer.IsActive)
            return Result.Success();

        customer.Activate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
