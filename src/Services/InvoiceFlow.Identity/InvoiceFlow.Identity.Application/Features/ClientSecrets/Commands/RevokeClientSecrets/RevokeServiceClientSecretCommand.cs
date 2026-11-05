using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record RevokeServiceClientSecretCommand(
    Guid ServiceClientId,
    Guid SecretId)
    : IRequest<Result>;
