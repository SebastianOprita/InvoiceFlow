using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Identity.Application;

public static class ApplicationErrors
{
    public static ApplicationError ConcurencyConflict => new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static ApplicationError DbSaveFailed => new(ApplicationErrorType.Internal, "persistence.save_failed", "A database error occurred while saving changes.");
    public static ApplicationError TenantNotFound => new(ApplicationErrorType.NotFound, "tenant.notFound", "Tenant not found.");
    public static ApplicationError TenantInactive => new(ApplicationErrorType.Forbidden, "tenant.inactive", "Tenant is inactive.");
    public static ApplicationError RoleNotFound => new(ApplicationErrorType.NotFound, "role.notFound", "Role not found.");
    public static ApplicationError UserUnauthorized => new(ApplicationErrorType.Unauthorized, "user.unauthorized", "User not found or invalid credentials.");
    public static ApplicationError UserNotFound => new(ApplicationErrorType.NotFound, "user.notFound", "User not found.");
    public static ApplicationError RoleNameAlreadyExists => new(ApplicationErrorType.Conflict, "role.name.alreadyExists", "Role name already exists for this tenant.");
    public static ApplicationError UserPasswordInvalid => new(ApplicationErrorType.Validation, "user.password.invalid", "Password is incorrect.");
    public static ApplicationError UserEmailAlreadyExists => new(ApplicationErrorType.Conflict, "user.email.alreadyExists", "User with same email already exists.");
    public static ApplicationError RefreshTokenNotFound => new(ApplicationErrorType.Unauthorized, "refreshToken.token.notFound", "Refresh token not found.");
    public static ApplicationError RefreshTokenInvalid => new(ApplicationErrorType.Unauthorized, "refreshToken.token.invalid", "Invalid or expired refresh token.");
}
