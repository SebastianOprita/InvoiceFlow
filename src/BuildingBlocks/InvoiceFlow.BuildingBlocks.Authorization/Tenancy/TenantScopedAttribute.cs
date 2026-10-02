using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.BuildingBlocks.Authorization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true)]
public sealed class TenantScopedAttribute : TypeFilterAttribute
{
    public TenantScopedAttribute()
        : base(typeof(TenantScopeFilter))
    {
    }
}
