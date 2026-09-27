using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class GetCustomerByIdQueryHandler(ICustomersRepository customersRepository) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery qry, CancellationToken cancellationToken)
    {
        var customer = await customersRepository.FindCustomerByIdAsync(qry.TenantId, qry.CustomerId, cancellationToken);

        if (customer is null)
            return Result<CustomerDto>.Failure(ApplicationErrors.CustomerNotFound);

        return Result<CustomerDto>.Success(customer.ToDto());
    }
}
