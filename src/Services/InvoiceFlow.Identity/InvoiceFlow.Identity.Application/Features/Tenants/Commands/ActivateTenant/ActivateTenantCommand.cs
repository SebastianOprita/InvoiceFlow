using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record ActivateTenantCommand(
    Guid TenantId)
    : IRequest<Result>;
