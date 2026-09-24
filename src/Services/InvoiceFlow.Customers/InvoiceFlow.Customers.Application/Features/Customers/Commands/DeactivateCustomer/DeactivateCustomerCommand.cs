using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public record DeactivateCustomerCommand(
    Guid TenantId,
    Guid CustomerId)
    : IRequest<Result>;
