namespace InvoiceFlow.Identity.Application;

public class TenantDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required DateTime? UpdatedAtUtc { get; set; }
}
