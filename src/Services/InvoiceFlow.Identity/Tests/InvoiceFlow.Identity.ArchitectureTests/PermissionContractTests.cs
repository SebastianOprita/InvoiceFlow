using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.ArchitectureTests;

public sealed class PermissionContractTests
{
    [Fact]
    public void RolePermissions_ShouldAcceptAllSystemPermissions()
    {
        var allPermissions = Enum
            .GetValues<SystemPermissions>()
            .Aggregate(
                0L,
                (current, permission) =>
                    current | (long)permission);

        var permissions = RolePermissions.Create(allPermissions);

        permissions.Value.Should().Be(allPermissions);
    }

    [Fact]
    public void RolePermissions_ShouldRejectUndefinedPermissions()
    {
        var allPermissions = Enum
            .GetValues<SystemPermissions>()
            .Aggregate(
                0L,
                (current, permission) =>
                    current | (long)permission);

        for (var bit = 0; bit < 63; bit++)
        {
            var permission = 1L << bit;

            if ((allPermissions & permission) != 0)
                continue;

            var act = () => RolePermissions.Create(permission);

            act.Should().Throw<DomainException>();
        }
    }
}
