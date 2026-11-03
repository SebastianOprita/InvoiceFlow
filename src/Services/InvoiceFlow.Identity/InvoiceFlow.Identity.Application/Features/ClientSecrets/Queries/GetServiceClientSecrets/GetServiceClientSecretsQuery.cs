using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GetServiceClientSecretsQuery(
    Guid ServiceClientId)
    : IRequest<Result<IReadOnlyList<ServiceClientSecretDto>>>;
