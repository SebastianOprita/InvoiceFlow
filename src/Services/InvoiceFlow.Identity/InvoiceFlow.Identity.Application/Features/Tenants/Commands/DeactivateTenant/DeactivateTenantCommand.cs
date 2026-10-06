using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record DeactivateTenantCommand(
    Guid TenantId)
    : IRequest<Result>;
