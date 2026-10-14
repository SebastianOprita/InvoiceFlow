using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public sealed record ActivateCustomerCommand(
    Guid TenantId,
    Guid CustomerId)
    : IRequest<Result>;
