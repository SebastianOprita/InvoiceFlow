using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record LogoutPlatformUserCommand(string RefreshToken)
    : IRequest<Result>;
