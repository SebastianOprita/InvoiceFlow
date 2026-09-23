using InvoiceFlow.Common.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public record GetCustomersQuery(
    Guid TenantId)
    : IRequest<Result<List<CustomerDto>>>;
