using InvoiceFlow.Identity.Application;

namespace InvoiceFlow.Identity.Api;

public static class Mapper
{
    public static CreateTenantCommand ToCommand(this CreateTenantRequest request)
    {
        return new CreateTenantCommand(
            request.Name,
            request.Slug
        );
    }

    public static UpdateTenantCommand ToCommand(this UpdateTenantRequest request, Guid tenantId)
    {
        return new UpdateTenantCommand(
            tenantId,
            request.Name
        );
    }

    public static CreatePlatformUserCommand ToCommand(this CreatePlatformUserRequest request)
    {
        return new CreatePlatformUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName
        );
    }

    public static UpdatePlatformUserCommand ToCommand(this UpdatePlatformUserRequest request, Guid userId)
    {
        return new UpdatePlatformUserCommand(
            userId,
            request.FirstName,
            request.LastName
        );
    }
}
