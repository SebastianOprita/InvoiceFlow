using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record CreatePlatformUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName)
    : IRequest<Result<PlatformUserDto>>;
