using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record UpdateUserCommand(
    Guid TenantId,
    Guid UserId,
    string FirstName,
    string LastName)
    : IRequest<Result<UserDto>>;
