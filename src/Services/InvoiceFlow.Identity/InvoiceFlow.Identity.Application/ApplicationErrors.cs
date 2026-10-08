using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Identity.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError TenantNotFound = new(ApplicationErrorType.NotFound, "tenant.notFound", "Tenant not found.");
    public static readonly ApplicationError TenantInactive = new(ApplicationErrorType.Forbidden, "tenant.inactive", "Tenant is inactive.");
    public static readonly ApplicationError RoleNotFound = new(ApplicationErrorType.NotFound, "role.notFound", "Role not found.");
    public static readonly ApplicationError UserUnauthorized = new(ApplicationErrorType.Unauthorized, "user.unauthorized", "User not found or invalid credentials.");
    public static readonly ApplicationError UserNotFound = new(ApplicationErrorType.NotFound, "user.notFound", "User not found.");
    public static readonly ApplicationError RoleNameAlreadyExists = new(ApplicationErrorType.Conflict, "roleName.alreadyExists", "Role name already exists for this tenant.");
    public static readonly ApplicationError UserPasswordInvalid = new(ApplicationErrorType.Validation, "userPassword.invalid", "Password is incorrect.");
    public static readonly ApplicationError UserEmailAlreadyExists = new(ApplicationErrorType.Conflict, "userEmail.alreadyExists", "User with same email already exists.");
    public static readonly ApplicationError RefreshTokenNotFound = new(ApplicationErrorType.Unauthorized, "refreshToken.notFound", "Refresh token not found.");
    public static readonly ApplicationError RefreshTokenInvalid = new(ApplicationErrorType.Unauthorized, "refreshToken.invalid", "Invalid or expired refresh token.");
    public static readonly ApplicationError TenantSlugAlreadyExists = new(ApplicationErrorType.Conflict, "tenantSlug.alreadyExists", "Tenant with same slug already exists.");
}
