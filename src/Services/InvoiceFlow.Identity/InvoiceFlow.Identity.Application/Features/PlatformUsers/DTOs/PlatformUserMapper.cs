using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public static class PlatformUserMapper
{
    public static PlatformUserDto ToDto(this PlatformUser platformUser)
    {
        return new PlatformUserDto
        {
            Id = platformUser.Id,
            Email = platformUser.Email.Value,
            FirstName = platformUser.FirstName.Value,
            LastName = platformUser.LastName.Value,
            IsActive = platformUser.IsActive,
            CreatedAtUtc = platformUser.CreatedAtUtc,
            UpdatedAtUtc = platformUser.UpdatedAtUtc
        };
    }
}
