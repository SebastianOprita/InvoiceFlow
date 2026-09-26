using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Identity.Application;

public static class ApplicationErrors
{
    public static ApplicationError ConcurencyConflict => new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static ApplicationError DbSaveFailed => new(ApplicationErrorType.Internal, "persistence.save_failed", "A database error occurred while saving changes.");
    public static ApplicationError TenantNotFound => new(ApplicationErrorType.NotFound, "tenant.notFound", "Tenant not found.");
    public static ApplicationError TenantInactive => new(ApplicationErrorType.Forbidden, "tenant.inactive", "Tenant is inactive.");
    public static ApplicationError RoleNotFound => new(ApplicationErrorType.NotFound, "role.notFound", "Role not found.");
    public static ApplicationError PlatformUserNotFound => new(ApplicationErrorType.NotFound, "platformUser.notFound", "Platform User not found.");
    public static ApplicationError LoginUserNotFound => new(ApplicationErrorType.Unauthorized, "login.user.notFound", "User not found or invalid credentials.");
    public static ApplicationError UserNotFound => new(ApplicationErrorType.NotFound, "user.notFound", "User not found.");
}
