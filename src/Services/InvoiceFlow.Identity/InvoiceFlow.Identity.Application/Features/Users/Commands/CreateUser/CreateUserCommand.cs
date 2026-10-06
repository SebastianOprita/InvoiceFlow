using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application.Features;

public record CreateUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName)
    : IRequest<Result<UserDto>>;
