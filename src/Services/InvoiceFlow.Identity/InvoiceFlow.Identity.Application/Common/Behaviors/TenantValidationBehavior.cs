using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InvoiceFlow.Identity.Application;

public sealed class TenantValidationBehavior<TRequest, TValue>
    : IPipelineBehavior<TRequest, Result<TValue>>
    where TRequest : ITenantScopedRequest
{
    private readonly ITenantsRepository _tenantsRepository;
    private readonly IHttpContextAccessor _accessor;

    public TenantValidationBehavior(ITenantsRepository tenantsRepository, IHttpContextAccessor accessor)
    {
        _tenantsRepository = tenantsRepository;
        _accessor = accessor;
    }

    public async Task<Result<TValue>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TValue>> next,
        CancellationToken cancellationToken)
    {
        var tenant = _tenantsRepository.GetTenantById(request.TenantId);

        if (tenant is null)
            return Result<TValue>.Failure(ApplicationErrors.TenantNotFound);

        if (!tenant.IsActive && _accessor.HttpContext?.User.FindFirstValue(InvoiceFlowClaimTypes.PrincipalType) == PrincipalTypes.TenantUser)
            return Result<TValue>.Failure(ApplicationErrors.TenantInactive);

        return await next();
    }
}
