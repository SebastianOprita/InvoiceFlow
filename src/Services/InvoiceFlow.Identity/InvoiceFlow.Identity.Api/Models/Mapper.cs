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
}
