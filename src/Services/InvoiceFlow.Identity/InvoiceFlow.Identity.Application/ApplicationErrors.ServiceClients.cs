using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Identity.Application;

public static partial class ApplicationErrors
{
    public static readonly ApplicationError ServiceClientNotFound =
        new(
            ApplicationErrorType.NotFound,
            "serviceClient.notFound",
            "Service client was not found.");

    public static readonly ApplicationError OrganizationNotFound =
        new(
            ApplicationErrorType.NotFound,
            "organization.notFound",
            "Organization was not found.");

    public static readonly ApplicationError OrganizationInactive =
        new(
            ApplicationErrorType.Conflict,
            "organization.inactive",
            "Organization is inactive.");

    public static readonly ApplicationError ServiceClientInactive =
        new(
            ApplicationErrorType.Conflict,
            "serviceClient.inactive",
            "Service client is inactive.");
}
