using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public sealed record GetCustomerByIdQuery(
    Guid TenantId,
    Guid CustomerId)
    : IRequest<Result<CustomerDto>>;
