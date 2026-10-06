using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record CreateTenantCommand(
    string Name,
    string Slug)
    : IRequest<Result<TenantDto>>;
