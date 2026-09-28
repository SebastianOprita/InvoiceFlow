namespace InvoiceFlow.BuildingBlocks.Api;

public sealed record ErrorCatalogueResponse(
    IReadOnlyCollection<ErrorDefinitionResponse> DomainErrors,
    IReadOnlyCollection<ErrorDefinitionResponse> ApplicationErrors);
