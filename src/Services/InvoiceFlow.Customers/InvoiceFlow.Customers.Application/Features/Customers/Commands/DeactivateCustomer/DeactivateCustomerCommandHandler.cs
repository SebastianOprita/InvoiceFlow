using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class DeactivateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    ICustomersRepository customersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeactivateCustomerCommand, Result>
{
    public async Task<Result> Handle(DeactivateCustomerCommand cmd, CancellationToken cancellationToken)
    {
        var customer = await customersRepository.GetCustomerByIdAsync(cmd.TenantId, cmd.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure(ApplicationErrors.DeactivateCustomerNotFound);

        if (!customer.IsActive)
            return Result.Success();

        customer.Deactivate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
