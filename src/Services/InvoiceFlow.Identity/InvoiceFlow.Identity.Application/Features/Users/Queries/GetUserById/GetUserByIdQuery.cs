using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetUserByIdQuery(
    Guid TenantId,
    Guid UserId)
    : IRequest<Result<UserDto>>;
