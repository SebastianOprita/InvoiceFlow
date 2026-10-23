namespace InvoiceFlow.Identity.Application;

public record RoleDto
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required long Permissions { get; set; }
    public required DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
