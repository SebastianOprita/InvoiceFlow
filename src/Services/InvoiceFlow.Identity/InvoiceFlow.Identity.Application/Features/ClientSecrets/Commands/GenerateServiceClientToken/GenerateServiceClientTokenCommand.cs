using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GenerateServiceClientTokenCommand(
    Guid TenantId,
    string ClientId,
    string ClientSecret)
    : IRequest<Result<GenerateServiceClientTokenCommandResponse>>;
