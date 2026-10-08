using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class GetCustomersQueryHandler(ICustomersRepository customersRepository) : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery qry, CancellationToken cancellationToken)
    {
        var customers = await customersRepository.GetAllCustomersAsync(qry.TenantId, cancellationToken);
        var customerDtos = customers.Select(c => c.ToDto()).ToList();

        return Result<List<CustomerDto>>.Success(customerDtos);
    }
}
