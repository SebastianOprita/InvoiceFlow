using InvoiceFlow.BuildingBlocks.Authorization;

namespace InvoiceFlow.Identity.Application;

public record RoleDto
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required SystemPermission Permissions { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
