using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record UpdatePlatformUserCommand(
    Guid UserId,
    string FirstName,
    string LastName)
    : IRequest<Result<PlatformUserDto>>;
