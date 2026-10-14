using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public sealed record GetCustomersQuery(
    Guid TenantId)
    : IRequest<Result<List<CustomerDto>>>;
