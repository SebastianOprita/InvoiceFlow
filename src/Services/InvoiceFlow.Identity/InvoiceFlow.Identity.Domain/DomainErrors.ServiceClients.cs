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

    public static readonly DomainError ServiceClientSecretNameRequired =
        new("secretName.required",
            "Service client secret name is required.");

    public static readonly DomainError ServiceClientSecretDescriptionRequired =
        new("secretDescription.required",
            "Service client secret description is required.");

    public static readonly DomainError ServiceClientSecretNameInvalid =
    new(
        "secretName.invalid",
        "Service client secret name must be between 1 and 100 characters.");

    public static readonly DomainError ServiceClientSecretDescriptionInvalid =
        new("secretDescription.invalid",
            "Service client secret description must be between 1 and 500 characters.");

    public static readonly DomainError ServiceClientSecretHashRequired =
        new("secretHash.required",
            "Service client secret hash is invalid.");
    public static readonly DomainError ServiceClientSecretHashInvalid =
        new("secretHash.invalid",
            "Service client secret hash is invalid.");

    public static readonly DomainError ServiceClientSecretNotFound =
        new(
            "secret.notFound",
            "Service client secret was not found.");

    public static readonly DomainError ServiceClientInactive =
        new(
            "secret.inactive",
            "Service client is inactive.");

    public static readonly DomainError ServiceClientSecretLimitReached =
    new(
        "secret.limitReached",
        $"Service client cannot have more than {ServiceClient.MaxActiveSecrets} active secrets.");

    public static readonly DomainError ServiceClientSecretAlreadyExists =
        new(
            "secret.alreadyExists",
            "Service client secret already exists.");
}
