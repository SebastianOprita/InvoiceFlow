using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record RevokeServiceClientPermissionsCommand(
    Guid ServiceClientId,
    long Permission)
     : IRequest<Result>;

