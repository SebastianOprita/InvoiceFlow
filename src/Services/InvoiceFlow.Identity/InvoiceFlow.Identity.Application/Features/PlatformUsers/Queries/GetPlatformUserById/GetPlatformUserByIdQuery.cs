using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetPlatformUserByIdQuery(
    Guid UserId)
    : IRequest<Result<PlatformUserDto>>;
