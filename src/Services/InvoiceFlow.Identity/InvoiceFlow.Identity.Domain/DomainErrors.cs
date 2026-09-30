using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public static class DomainErrors
{
    public static DomainError EmailRequired => new("email.required", "Email is required.");
    public static DomainError EmailTooLong => new("email.tooLong", $"Email is too long. Maximum length is {UserEmail.MaxLength} characters.");
    public static DomainError EmailInvalid => new("email.invalid", "Email is invalid. Should contain '@' and should not start or end with '@'.");
    public static DomainError PasswordHashRequired => new("passwordHash.required", "Password Hash is required.");
    public static DomainError FirstNameRequired => new("firstName.required", "First Name is required.");
    public static DomainError FirstNameTooLong => new("firstName.tooLong", $"First Name is too long. Maximum length is {FirstName.MaxLength} characters.");
    public static DomainError LastNameRequired => new("lastName.required", "Last Name is required.");
    public static DomainError LastNameTooLong => new("lastName.tooLong", $"Last Name is too long. Maximum length is {LastName.MaxLength} characters.");
    public static DomainError TenantIdRequired => new("tenantId.required", "Tenant Id is required.");
    public static DomainError IdRequired => new("id.required", "Id is required.");
    public static DomainError UserIdRequired => new("userId.required", "User Id is required.");
    public static DomainError RoleIdRequired => new("roleId.required", "Role Id is required.");
    public static DomainError CreatedAtUtcRequired => new("createdAtUtc.required", "Created datetime is required.");
    public static DomainError CreatedAtUtcNotUtc => new("createdAtUtc.notUtc", "Created datetime must be in UTC.");
    public static DomainError UpdatedAtUtcRequired => new("updatedAtUtc.required", "Updated datetime is required.");
    public static DomainError UpdatedAtUtcNotUtc => new("updatedAtUtc.notUtc", "Updated datetime must be in UTC.");
    public static DomainError UpdatedAtUtcInvalid => new("updatedAtUtc.invalid", "Updated datetime cannot be earlier than Created datetime.");
    public static DomainError AssignedAtUtcRequired => new("assignedAtUtc.required", "Assigned datetime is required.");
    public static DomainError AssignedAtUtcNotUtc => new("assignedAtUtc.notUtc", "Assigned datetime must be in UTC.");
    public static DomainError PermissionInvalid => new("permission.invalid", "Permission contains invalid flags.");
    public static DomainError PermissionRequired => new("permission.required", "Permission is required.");
    public static DomainError DescriptionRequired => new("description.required", "Description is required.");
    public static DomainError DescriptionTooLong => new("description.tooLong", $"Description is too long. Maximum length is {RoleDescription.MaxLength} characters.");
    public static DomainError NameRequired => new("name.required", "Name is required.");
    public static DomainError NameTooLong => new("name.tooLong", $"Name is too long. Maximum length is {RoleName.MaxLength} characters.");
    public static DomainError SlugRequired => new("slug.required", "Slug is required.");
    public static DomainError SlugTooLong => new("slug.tooLong", $"Slug is too long. Maximum length is {TenantSlug.MaxLength} characters.");
    public static DomainError SlugInvalid => new("slug.invalid", "Slug may contain only lowercase letters, digits, and '-'.");
    public static DomainError SlugStartsOrEndsInvalid => new("slug.startsOrEndsInvalid", "Slug cannot start or end with '-'.");
    public static DomainError TokenHashRequired => new("tokenHash.required", "Token Hash is required.");
    public static DomainError TokenHashTooLong => new("tokenHash.tooLong", $"Token Hash is too long. Maximum length is {RefreshTokenHash.MaxLength} characters.");
    public static DomainError DeviceInfoRequired => new("deviceInfo.required", "Device Info is required.");
    public static DomainError DeviceInfoTooLong => new("deviceInfo.tooLong", $"Device Info is too long. Maximum length is {DeviceInfo.MaxLength} characters.");
    public static DomainError IpAddressRequired => new("ipAddress.required", "Ip Address is required.");
    public static DomainError IpAddressTooLong => new("ipAddress.tooLong", $"Ip Address is too long. Maximum length is {IpAddress.MaxLength} characters.");
    public static DomainError ExpiresAtUtcNotUtc => new("expiresAtUtc.notUtc", $"Expiry datetime must be UTC.");
    public static DomainError ExpiresAtUtcInvalid => new("expiresAtUtc.invalid", "Expiry datetime must be in the future.");
    public static DomainError ExpiresAtUtcRequired => new("expiresAtUtc.required", "Expiry datetime is required.");
    public static DomainError RevokedAtUtcRequired => new("revokedAtUtc.required", "Revoked datetime is required.");
    public static DomainError RevokedAtUtcInvalid => new("revokedAtUtc.invalid", "Revocation time cannot be earlier than creation time.");
    public static DomainError RevokedAtUtcNotUtc => new("revokedAtUtc.notUtc", $"Revoked datetime must be UTC.");
    public static DomainError RefreshTokensRevoked => new("refreshTokens.revoked", "Cannot rotate a revoked refresh token.");
    public static DomainError RefreshTokensExpired => new("refreshTokens.expired", "Cannot rotate an expired refresh token.");
}
