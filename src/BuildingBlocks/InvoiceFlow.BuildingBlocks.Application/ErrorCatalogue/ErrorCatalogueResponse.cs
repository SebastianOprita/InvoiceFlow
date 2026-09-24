namespace InvoiceFlow.BuildingBlocks.Application;

public sealed record ErrorCatalogueResponse(
    IReadOnlyCollection<ErrorDefinitionResponse> DomainErrors,
    IReadOnlyCollection<ErrorDefinitionResponse> ApplicationErrors)
{
    public static ErrorCatalogueResponse GetErrorCatalogueResponse(
        Type domainErrorsType,
        Type applicationErrorsType)
    {
        return new ErrorCatalogueResponse(
            DomainErrors: ErrorCatalogue.GetDomainErrors(domainErrorsType),
            ApplicationErrors: ErrorCatalogue.GetApplicationErrors(applicationErrorsType));
    }
}
