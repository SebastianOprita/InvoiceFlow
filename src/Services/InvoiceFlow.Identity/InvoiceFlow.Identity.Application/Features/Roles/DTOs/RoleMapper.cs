using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public static class RoleMapper
{
    public static RoleDto ToDto(this Role role)
    {
        return new RoleDto
        {
            TenantId = role.TenantId,
            Id = role.Id,
            Name = role.Name.Value,
            Description = role.Description?.Value,
            Permissions = role.Permissions.Value,
            CreatedAtUtc = role.CreatedAtUtc,
            UpdatedAtUtc = role.UpdatedAtUtc
        };
    }
}
