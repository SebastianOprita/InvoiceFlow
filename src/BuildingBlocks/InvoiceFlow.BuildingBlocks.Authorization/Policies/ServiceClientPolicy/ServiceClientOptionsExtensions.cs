using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization;

public static class ServiceClientOptionsExtensions
{
    public static AuthorizationOptions AddServiceClientLoginPolicy(
        this AuthorizationOptions options)
    {
        options.AddPolicy(
            ServiceClientPolicy.PolicyName,
            ServiceClientPolicy.Build());

        return options;
    }
}
