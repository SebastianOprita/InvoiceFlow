using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record ActivateTenantCommand(
    Guid TenantId)
    : IRequest<Result>;
