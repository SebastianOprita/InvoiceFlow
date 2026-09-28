using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(
            PermissionPolicy.Prefix,
            StringComparison.OrdinalIgnoreCase))
        {
            var value = policyName[
                PermissionPolicy.Prefix.Length..];

            if (long.TryParse(value, out var raw))
            {
                var permission = (SystemPermission)raw;

                if (permission.IsValid() &&
                    permission != SystemPermission.None)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddRequirements(
                            new PermissionRequirement(permission))
                        .Build();

                    return Task.FromResult<AuthorizationPolicy?>(policy);
                }
            }
        }

        return base.GetPolicyAsync(policyName);
    }
}
