using InvoiceFlow.BuildingBlocks.Application;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InvoiceFlow.Identity.Application;

public sealed class TenantValidationBehavior<TRequest, TValue>
    (ITenantsRepository tenantsRepository, IHttpContextAccessor accessor)
    : IPipelineBehavior<TRequest, Result<TValue>>
    where TRequest : ITenantScopedCommand
{
    public async Task<Result<TValue>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TValue>> next,
        CancellationToken ct)
    {
        var tenant = tenantsRepository.GetTenantById(request.TenantId);

        if (tenant is null)
            return Result<TValue>.Failure(new ApplicationError(ApplicationErrorType.NotFound, "tenant.NotFound", "Tenant was not found."));

        if (!tenant.IsActive && accessor.HttpContext?.User.FindFirstValue("principal_type") == "tenant_user")
            return Result<TValue>.Failure(new ApplicationError(ApplicationErrorType.Forbidden, "tenant.inactive", "Tenant is inactive."));

        return await next();
    }
}
