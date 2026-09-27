using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.BuildingBlocks.Authorization;

[ExcludeFromCodeCoverage]
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
