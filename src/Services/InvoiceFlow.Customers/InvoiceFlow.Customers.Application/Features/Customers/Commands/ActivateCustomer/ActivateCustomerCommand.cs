using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public record ActivateCustomerCommand(
    Guid TenantId,
    Guid CustomerId)
    : IRequest<Result>;
