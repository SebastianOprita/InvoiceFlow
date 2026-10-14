using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public sealed record DeactivateCustomerCommand(
    Guid TenantId,
    Guid CustomerId)
    : IRequest<Result>;
