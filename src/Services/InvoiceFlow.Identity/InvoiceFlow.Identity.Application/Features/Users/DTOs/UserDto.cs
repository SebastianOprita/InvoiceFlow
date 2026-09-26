namespace InvoiceFlow.Identity.Application;

public record UserDto
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
