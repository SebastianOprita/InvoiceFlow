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

    public static CreatePlatformUserCommand ToCommand(this CreateUserRequest request)
    {
        return new CreatePlatformUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName
        );
    }

    public static UpdatePlatformUserCommand ToCommand(this UpdateUserRequest request, Guid userId)
    {
        return new UpdatePlatformUserCommand(
            userId,
            request.FirstName,
            request.LastName
        );
    }

    public static CreateUserCommand ToCommand(this CreateUserRequest request, Guid tenantId)
    {
        return new CreateUserCommand(
            tenantId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName
        );
    }

    public static UpdateUserCommand ToCommand(this UpdateUserRequest request, Guid tenantId, Guid userId)
    {
        return new UpdateUserCommand(
            tenantId,
            userId,
            request.FirstName,
            request.LastName
        );
    }

    public static AssignRoleCommand ToCommand(this AssignRoleRequest request, Guid tenantId, Guid userId)
    {
        return new AssignRoleCommand(
            tenantId,
            userId,
            request.RoleId
        );
    }
}
