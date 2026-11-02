using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ServiceClientAttribute : AuthorizeAttribute
{
    public ServiceClientAttribute()
    {
        Policy = ServiceClientPolicy.PolicyName;
    }
}
