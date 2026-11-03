using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetServiceClientSecretsQueryHandler(
    IServiceClientsRepository serviceClientsRepository)
    : IRequestHandler<GetServiceClientSecretsQuery,
        Result<IReadOnlyList<ServiceClientSecretDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceClientSecretDto>>> Handle(
        GetServiceClientSecretsQuery query,
        CancellationToken cancellationToken)
    {
        var serviceClient = await serviceClientsRepository.GetServiceClientByIdAsync(query.ServiceClientId, cancellationToken);

        if (serviceClient is null)
            return Result<IReadOnlyList<ServiceClientSecretDto>>.Failure(
                ApplicationErrors.ServiceClientNotFound);

        IReadOnlyList<ServiceClientSecretDto> secrets = serviceClient.Secrets
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => x.ToDto())
            .ToList();

        return Result<IReadOnlyList<ServiceClientSecretDto>>.Success(secrets);
    }
}
