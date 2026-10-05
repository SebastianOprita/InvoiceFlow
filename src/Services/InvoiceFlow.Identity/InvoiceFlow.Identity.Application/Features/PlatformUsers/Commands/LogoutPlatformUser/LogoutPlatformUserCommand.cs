using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record LogoutPlatformUserCommand(string RefreshToken)
    : IRequest<Result>;
