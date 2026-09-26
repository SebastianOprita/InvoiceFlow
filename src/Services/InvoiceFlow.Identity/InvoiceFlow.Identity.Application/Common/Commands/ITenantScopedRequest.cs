namespace InvoiceFlow.Identity.Application;

public interface ITenantScopedRequest
{
    Guid TenantId { get; }
}
