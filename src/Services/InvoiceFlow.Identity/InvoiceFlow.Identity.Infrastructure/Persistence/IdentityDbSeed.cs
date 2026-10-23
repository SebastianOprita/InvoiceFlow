using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Infrastructure;

public static class IdentityDbSeed
{
    public static async Task SeedAsync(IdentityDbContext db, ISystemDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken = default)
    {
        var tenant = Tenant.Create(
            Guid.Parse("f305e3f0-e634-470b-b9c1-cc0c7559dc0a"),
            TenantName.Create("Default Tenant"),
            TenantSlug.Create("default-tenant"),
            dateTimeProvider.GetUtcNow());

        var user = User.Create(
            tenant.Id,
            Guid.CreateVersion7(),
            UserEmail.Create("admin@email.com"),
            PasswordHash.Create("AQAAAAIAAYagAAAAEBTiBEyF1jroUbr5ZzCizb1/uF2BQWtZXW8Y1kifHlGvJIlI4rhWElB4V49dlY2+Ig=="),
            FirstName.Create("admin"),
            LastName.Create("admin"),
            dateTimeProvider.GetUtcNow());

        var role = Role.Create(
            tenant.Id,
            Guid.CreateVersion7(),
            RoleName.Create("Admin"),
            dateTimeProvider.GetUtcNow(),
            RoleDescription.CreateOptional("Administration role with full permissions."));
        role.GrantPermission(1L << 3, dateTimeProvider.GetUtcNow());
        role.GrantPermission(1L << 4, dateTimeProvider.GetUtcNow());
        role.GrantPermission(1L << 5, dateTimeProvider.GetUtcNow());
        role.GrantPermission(1L << 6, dateTimeProvider.GetUtcNow());
        role.GrantPermission(1L << 1, dateTimeProvider.GetUtcNow());
        role.GrantPermission(1L << 0, dateTimeProvider.GetUtcNow());

        user.AssignRole(role.Id, dateTimeProvider.GetUtcNow());

        await db.Tenants.AddAsync(tenant);
        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.SaveChangesAsync(cancellationToken);
    }
}
