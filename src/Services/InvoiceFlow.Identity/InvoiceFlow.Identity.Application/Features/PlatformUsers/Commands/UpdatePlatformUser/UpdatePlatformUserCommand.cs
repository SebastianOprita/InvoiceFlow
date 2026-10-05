using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record UpdatePlatformUserCommand(
    Guid UserId,
    string FirstName,
    string LastName)
    : IRequest<Result<PlatformUserDto>>;
