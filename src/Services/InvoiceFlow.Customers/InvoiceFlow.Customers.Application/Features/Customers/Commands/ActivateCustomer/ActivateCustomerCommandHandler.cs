using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class ActivateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    ICustomersRepository customersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ActivateCustomerCommand, Result>
{
    public async Task<Result> Handle(ActivateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = customersRepository.GetCustomerById(cmd.TenantId, cmd.CustomerId);
        if (customer is null)
            return Result.Failure(ApplicationErrors.ActivateCustomerNotFound);

        if (customer.IsActive)
            return Result.Success();

        customer.Activate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(ct);
        return result;
    }
}
