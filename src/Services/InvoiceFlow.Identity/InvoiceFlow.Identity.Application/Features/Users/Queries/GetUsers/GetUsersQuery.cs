using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetUsersQuery(
    Guid TenantId)
    : IRequest<Result<List<UserDto>>>, ITenantScopedRequest;
