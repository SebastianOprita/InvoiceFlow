using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GetPlatformUsersQuery
    : IRequest<Result<List<PlatformUserDto>>>;
