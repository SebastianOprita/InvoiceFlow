using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using System.Globalization;
using System.Security.Claims;

namespace InvoiceFlow.BuildingBlocks.Authorization.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal
            .FindFirst(InvoiceFlowClaimTypes.UserId)?
            .Value;

        return Guid.TryParse(value, out var userId)
            ? userId
            : null;
    }

    public static Guid? GetTenantId(this ClaimsPrincipal principal)
    {
        var value = principal
            .FindFirst(InvoiceFlowClaimTypes.TenantId)?
            .Value;

        return Guid.TryParse(value, out var tenantId)
            ? tenantId
            : null;
    }

    public static SystemPermission GetPermissions(
        this ClaimsPrincipal principal)
    {
        var value = principal
            .FindFirst(InvoiceFlowClaimTypes.Permissions)?
            .Value;

        if (!long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var raw))
        {
            return SystemPermission.None;
        }

        var permissions = (SystemPermission)raw;

        return permissions.IsValid()
            ? permissions
            : SystemPermission.None;
    }

    public static bool HasPermission(
        this ClaimsPrincipal principal,
        SystemPermission permission)
    {
        var permissions = principal.GetPermissions();

        return (permissions & permission) == permission;
    }

    public static bool IsImpersonating(
        this ClaimsPrincipal principal)
    {
        return principal
            .FindFirst(InvoiceFlowClaimTypes.AuthMode)?
            .Value
            == AuthenticationModes.Impersonation;
    }
}
