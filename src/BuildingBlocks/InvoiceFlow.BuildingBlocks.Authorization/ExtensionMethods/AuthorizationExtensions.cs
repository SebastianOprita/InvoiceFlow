using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddInvoiceFlowAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddTenantUserLoginPolicy();
            options.AddPlatformUserLoginPolicy();
        });

        services.AddScoped<TenantScopeFilter>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
