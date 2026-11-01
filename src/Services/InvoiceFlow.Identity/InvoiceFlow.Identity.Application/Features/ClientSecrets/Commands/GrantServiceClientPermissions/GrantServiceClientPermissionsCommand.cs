using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GrantServiceClientPermissionsCommand(
    Guid ServiceClientId,
    long Permission)
     : IRequest<Result>;
