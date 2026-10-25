using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
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
        role.GrantPermission((long)SystemPermissions.CustomerRead, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.CustomerCreate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.CustomerUpdate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.UserRead, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.UserCreate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.UserUpdate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.UserStatusManage, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.UserRoleManage, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.RoleRead, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.RoleCreate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.RoleUpdate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.RoleStatusManage, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.RolePermissionsUpdate, dateTimeProvider.GetUtcNow());

        role.GrantPermission((long)SystemPermissions.ProductRead, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.ProductCreate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.ProductUpdate, dateTimeProvider.GetUtcNow());
        role.GrantPermission((long)SystemPermissions.ProductStatusManage, dateTimeProvider.GetUtcNow());
        user.AssignRole(role.Id, dateTimeProvider.GetUtcNow());

        await db.Tenants.AddAsync(tenant);
        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.SaveChangesAsync(cancellationToken);
    }
}
