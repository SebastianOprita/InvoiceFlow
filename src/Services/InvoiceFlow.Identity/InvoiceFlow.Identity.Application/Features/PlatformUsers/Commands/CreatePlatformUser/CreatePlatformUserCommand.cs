using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record CreatePlatformUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName)
    : IRequest<Result<PlatformUserDto>>;
