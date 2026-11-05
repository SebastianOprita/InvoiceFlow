using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record CreateServiceClientSecretCommand(
    Guid ServiceClientId,
    string Name,
    string? Description,
    DateTimeOffset ExpiresAtUtc)
    : IRequest<Result<CreateServiceClientSecretCommandResponse>>;
