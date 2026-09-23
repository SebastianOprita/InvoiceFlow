using InvoiceFlow.Common.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public class GetCustomersQueryHandler(ICustomersRepository customersRepository) : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery qry, CancellationToken ct)
    {
        var customers = customersRepository.FindAllCustomers(qry.TenantId)
            .Select(c => c.ToDto())
            .ToList();

        return Result<List<CustomerDto>>.Success(customers);
    }
}
