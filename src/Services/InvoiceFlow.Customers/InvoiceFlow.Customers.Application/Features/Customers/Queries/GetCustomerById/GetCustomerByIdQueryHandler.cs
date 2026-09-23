using InvoiceFlow.Common.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class GetCustomerByIdQueryHandler(ICustomersRepository customersRepository) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery qry, CancellationToken ct)
    {
        var customer = customersRepository.FindCustomerById(qry.TenantId, qry.CustomerId);

        if (customer is null)
            return Result<CustomerDto>.Failure(ApplicationErrors.CustomerNotFound);

        return Result<CustomerDto>.Success(customer.ToDto());
    }
}
