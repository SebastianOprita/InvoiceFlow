using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public class UserRoleTests
{
    private static readonly Guid TenantId = Guid.CreateVersion7();
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid RoleId = Guid.CreateVersion7();
    private static readonly DateTime AssignedAtUtc =
        new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_Should_CreateUserRole_WithValidValues()
    {
        var userRole = UserRole.Create(
            TenantId,
            UserId,
            RoleId,
            AssignedAtUtc);

        userRole.TenantId.Should().Be(TenantId);
        userRole.UserId.Should().Be(UserId);
        userRole.RoleId.Should().Be(RoleId);
        userRole.AssignedAtUtc.Should().Be(AssignedAtUtc);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenTenantIdIsEmpty()
    {
        var act = () => UserRole.Create(
            Guid.Empty,
            UserId,
            RoleId,
            AssignedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.TenantIdRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenUserIdIsEmpty()
    {
        var act = () => UserRole.Create(
            TenantId,
            Guid.Empty,
            RoleId,
            AssignedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.UserIdRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenRoleIdIsEmpty()
    {
        var act = () => UserRole.Create(
            TenantId,
            UserId,
            Guid.Empty,
            AssignedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RoleIdRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenAssignedAtUtcIsDefault()
    {
        var act = () => UserRole.Create(
            TenantId,
            UserId,
            RoleId,
            default);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.AssignedAtUtcRequired.ErrorMessage);
    }
}
