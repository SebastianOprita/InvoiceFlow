using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record DeactivateTenantCommand(
    Guid TenantId)
    : IRequest<Result>;
