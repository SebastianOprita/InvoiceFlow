namespace InvoiceFlow.Identity.Application;

public interface ITenantScopedCommand
{
    Guid TenantId { get; }
}
