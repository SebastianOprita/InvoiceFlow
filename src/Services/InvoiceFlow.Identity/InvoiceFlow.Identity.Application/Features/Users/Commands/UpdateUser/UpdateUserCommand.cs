using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record UpdateUserCommand(
    Guid TenantId,
    Guid UserId,
    string FirstName,
    string LastName)
    : IRequest<Result<UserDto>>;
