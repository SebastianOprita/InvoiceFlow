using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Identity.Application;

public static class ApplicationErrors
{
    public static class Users
    {
        public static ApplicationError LoginUserNotFound => new(ApplicationErrorType.Unauthorized, "login.user.notFound", "User not found or invalid credentials.");
    }
}
