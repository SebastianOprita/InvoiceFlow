using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetPlatformUsersQuery
    : IRequest<Result<List<PlatformUserDto>>>;
