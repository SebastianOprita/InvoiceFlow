using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError OrganizationNameRequired =
        new("organizationName.required",
            "Organization name must is required.");
    public static readonly DomainError OrganizationNameInvalid =
        new("organizationName.invalid",
            "Organization name must be between 1 and 200 characters.");
    public static readonly DomainError ServiceClientIdRequired =
        new("serviceClientId.required",
            "Service client Id is required.");
    public static readonly DomainError ServiceClientIdInvalid =
        new("serviceClientId.Invalid",
            "Service client Id must contain 1-100 lowercase letters, numbers, hyphens or underscores.");

    public static readonly DomainError ServiceClientNameRequired =
        new("serviceClientName.required",
            "Service client name is required.");
    public static readonly DomainError ServiceClientNameInvalid =
        new("serviceClientName.invalid",
            "Service client name must be between 1 and 150 characters.");

    public static readonly DomainError OrganizationIdRequired =
        new("organizationId.required", "Organization ID is required.");

    public static readonly DomainError OrganizationTypeInvalid =
        new("organizationType.invalid", "Organization type is invalid.");

    public static readonly DomainError OrganizationInactive =
        new("organization.inactive", "Organization is inactive.");
}
