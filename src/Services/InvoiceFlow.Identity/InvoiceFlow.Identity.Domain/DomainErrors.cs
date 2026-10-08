using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public static class DomainErrors
{
    public static readonly DomainError EmailRequired = new("email.required", "Email is required.");
    public static readonly DomainError EmailTooLong = new("email.tooLong", $"Email is too long. Maximum length is {UserEmail.MaxLength} characters.");
    public static readonly DomainError EmailInvalid = new("email.invalid", "Email is invalid. Should contain '@' and should not start or end with '@'.");
    public static readonly DomainError PasswordHashRequired = new("passwordHash.required", "Password Hash is required.");
    public static readonly DomainError FirstNameRequired = new("firstName.required", "First Name is required.");
    public static readonly DomainError FirstNameTooLong = new("firstName.tooLong", $"First Name is too long. Maximum length is {FirstName.MaxLength} characters.");
    public static readonly DomainError LastNameRequired = new("lastName.required", "Last Name is required.");
    public static readonly DomainError LastNameTooLong = new("lastName.tooLong", $"Last Name is too long. Maximum length is {LastName.MaxLength} characters.");
    public static readonly DomainError TenantIdRequired = new("tenantId.required", "Tenant Id is required.");
    public static readonly DomainError IdRequired = new("id.required", "Id is required.");
    public static readonly DomainError UserIdRequired = new("userId.required", "User Id is required.");
    public static readonly DomainError RoleIdRequired = new("roleId.required", "Role Id is required.");
    public static readonly DomainError CreatedAtUtcRequired = new("createdAtUtc.required", "Created datetime is required.");
    public static readonly DomainError CreatedAtUtcNotUtc = new("createdAtUtc.notUtc", "Created datetime must be in UTC.");
    public static readonly DomainError UpdatedAtUtcRequired = new("updatedAtUtc.required", "Updated datetime is required.");
    public static readonly DomainError UpdatedAtUtcNotUtc = new("updatedAtUtc.notUtc", "Updated datetime must be in UTC.");
    public static readonly DomainError UpdatedAtUtcInvalid = new("updatedAtUtc.invalid", "Updated datetime cannot be earlier than Created datetime.");
    public static readonly DomainError AssignedAtUtcRequired = new("assignedAtUtc.required", "Assigned datetime is required.");
    public static readonly DomainError AssignedAtUtcNotUtc = new("assignedAtUtc.notUtc", "Assigned datetime must be in UTC.");
    public static readonly DomainError PermissionInvalid = new("permission.invalid", "Permission contains invalid flags.");
    public static readonly DomainError PermissionRequired = new("permission.required", "Permission is required.");
    public static readonly DomainError DescriptionRequired = new("description.required", "Description is required.");
    public static readonly DomainError DescriptionTooLong = new("description.tooLong", $"Description is too long. Maximum length is {RoleDescription.MaxLength} characters.");
    public static readonly DomainError NameRequired = new("name.required", "Name is required.");
    public static readonly DomainError NameTooLong = new("name.tooLong", $"Name is too long. Maximum length is {RoleName.MaxLength} characters.");
    public static readonly DomainError SlugRequired = new("slug.required", "Slug is required.");
    public static readonly DomainError SlugTooLong = new("slug.tooLong", $"Slug is too long. Maximum length is {TenantSlug.MaxLength} characters.");
    public static readonly DomainError SlugInvalid = new("slug.invalid", "Slug may contain only lowercase letters, digits, and '-'.");
    public static readonly DomainError SlugStartsOrEndsInvalid = new("slug.startsOrEndsInvalid", "Slug cannot start or end with '-'.");
    public static readonly DomainError TokenHashRequired = new("tokenHash.required", "Token Hash is required.");
    public static readonly DomainError TokenHashTooLong = new("tokenHash.tooLong", $"Token Hash is too long. Maximum length is {RefreshTokenHash.MaxLength} characters.");
    public static readonly DomainError DeviceInfoRequired = new("deviceInfo.required", "Device Info is required.");
    public static readonly DomainError DeviceInfoTooLong = new("deviceInfo.tooLong", $"Device Info is too long. Maximum length is {DeviceInfo.MaxLength} characters.");
    public static readonly DomainError IpAddressRequired = new("ipAddress.required", "Ip Address is required.");
    public static readonly DomainError IpAddressTooLong = new("ipAddress.tooLong", $"Ip Address is too long. Maximum length is {IpAddress.MaxLength} characters.");
    public static readonly DomainError ExpiresAtUtcNotUtc = new("expiresAtUtc.notUtc", $"Expiry datetime must be UTC.");
    public static readonly DomainError ExpiresAtUtcInvalid = new("expiresAtUtc.invalid", "Expiry datetime must be in the future.");
    public static readonly DomainError ExpiresAtUtcRequired = new("expiresAtUtc.required", "Expiry datetime is required.");
    public static readonly DomainError RevokedAtUtcRequired = new("revokedAtUtc.required", "Revoked datetime is required.");
    public static readonly DomainError RevokedAtUtcInvalid = new("revokedAtUtc.invalid", "Revocation time cannot be earlier than creation time.");
    public static readonly DomainError RevokedAtUtcNotUtc = new("revokedAtUtc.notUtc", $"Revoked datetime must be UTC.");
    public static readonly DomainError RefreshTokensRevoked = new("refreshTokens.revoked", "Cannot rotate a revoked refresh token.");
    public static readonly DomainError RefreshTokensExpired = new("refreshTokens.expired", "Cannot rotate an expired refresh token.");
}
