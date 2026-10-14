using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name)
    : IRequest<Result<TenantDto>>;
