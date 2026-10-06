using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record UpdateTenantCommand(
    Guid TenantId,
    string Name)
    : IRequest<Result<TenantDto>>;
