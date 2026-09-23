using InvoiceFlow.Common.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public record GetCustomerByIdQuery(
    Guid TenantId,
    Guid CustomerId)
    : IRequest<Result<CustomerDto>>;
