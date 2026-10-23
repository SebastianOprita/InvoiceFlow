namespace InvoiceFlow.Identity.Application;

public class TenantDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required bool IsActive { get; set; }
    public required DateTimeOffset CreatedAtUtc { get; set; }
    public required DateTimeOffset? UpdatedAtUtc { get; set; }
}
