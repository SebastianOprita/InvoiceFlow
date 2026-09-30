using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public class PlatformUserTests
{
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly DateTime CreatedAtUtc = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAtUtc = CreatedAtUtc.AddHours(1);

    [Fact]
    public void Constructor_Should_CreateUser_WithValidValues()
    {
        var email = UserEmail.Create("john.doe@test.com");
        var passwordHash = PasswordHash.Create("hashed-password");
        var firstName = FirstName.Create("John");
        var lastName = LastName.Create("Doe");

        var user = PlatformUser.Create(
            UserId,
            email,
            passwordHash,
            firstName,
            lastName,
            CreatedAtUtc);

        user.Id.Should().Be(UserId);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
        user.CreatedAtUtc.Should().Be(CreatedAtUtc);
        user.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_Throw_WhenIdIsEmpty()
    {
        var act = () => PlatformUser.Create(
            Guid.Empty,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashed-password"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            CreatedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.IdRequired.ErrorMessage);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenCreatedAtUtcIsDefault()
    {
        var act = () => PlatformUser.Create(
            UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashed-password"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            default);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CreatedAtUtcRequired.ErrorMessage);
    }

    [Fact]
    public void UpdateProfile_Should_UpdateFirstNameLastNameAndUpdatedAtUtc()
    {
        var user = CreateUser();
        var firstName = FirstName.Create("Jane");
        var lastName = LastName.Create("Smith");

        user.UpdateProfile(firstName, lastName, UpdatedAtUtc);

        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void UpdateProfile_Should_NotUpdateUpdatedAtUtc_WhenProfileIsUnchanged()
    {
        var firstName = FirstName.Create("John");
        var lastName = LastName.Create("Doe");
        var user = CreateUser(firstName: firstName, lastName: lastName);

        user.UpdateProfile(firstName, lastName, UpdatedAtUtc);

        user.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangeEmail_Should_UpdateEmailAndUpdatedAtUtc()
    {
        var user = CreateUser();
        var email = UserEmail.Create("jane.smith@test.com");

        user.ChangeEmail(email, UpdatedAtUtc);

        user.Email.Should().Be(email);
        user.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void ChangeEmail_Should_NotUpdateUpdatedAtUtc_WhenEmailIsUnchanged()
    {
        var email = UserEmail.Create("john.doe@test.com");
        var user = CreateUser(email: email);

        user.ChangeEmail(email, UpdatedAtUtc);

        user.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangePasswordHash_Should_UpdatePasswordHashAndUpdatedAtUtc()
    {
        var user = CreateUser();
        var passwordHash = PasswordHash.Create("new-hashed-password");

        user.ChangePasswordHash(passwordHash, UpdatedAtUtc);

        user.PasswordHash.Should().Be(passwordHash);
        user.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void ChangePasswordHash_Should_NotUpdateUpdatedAtUtc_WhenPasswordHashIsUnchanged()
    {
        var passwordHash = PasswordHash.Create("hashed-password");
        var user = CreateUser(passwordHash: passwordHash);

        user.ChangePasswordHash(passwordHash, UpdatedAtUtc);

        user.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse_AndUpdateUpdatedAtUtc()
    {
        var user = CreateUser();

        user.Deactivate(UpdatedAtUtc);

        user.IsActive.Should().BeFalse();
        user.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Deactivate_Should_NotUpdateUpdatedAtUtc_WhenAlreadyInactive()
    {
        var user = CreateUser();

        user.Deactivate(UpdatedAtUtc);

        user.Deactivate(UpdatedAtUtc.AddHours(1));

        user.IsActive.Should().BeFalse();
        user.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Activate_Should_SetIsActiveToTrue_AndUpdateUpdatedAtUtc()
    {
        var user = CreateUser();
        user.Deactivate(UpdatedAtUtc);

        var activatedAtUtc = UpdatedAtUtc.AddHours(1);

        user.Activate(activatedAtUtc);

        user.IsActive.Should().BeTrue();
        user.UpdatedAtUtc.Should().Be(activatedAtUtc);
    }

    [Fact]
    public void Activate_Should_NotUpdateUpdatedAtUtc_WhenAlreadyActive()
    {
        var user = CreateUser();

        user.Activate(UpdatedAtUtc);

        user.IsActive.Should().BeTrue();
        user.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(UpdateActionsWithDefaultDate))]
    public void UpdateMethods_Should_Throw_WhenUpdatedAtUtcIsDefault(Action<PlatformUser> updateAction)
    {
        var user = CreateUser();

        var act = () => updateAction(user);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.UpdatedAtUtcRequired.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(UpdateActionsWithInvalidDate))]
    public void UpdateMethods_Should_Throw_WhenUpdatedAtUtcIsEarlierThanCreatedAtUtc(Action<PlatformUser> updateAction)
    {
        var user = CreateUser();

        var act = () => updateAction(user);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.UpdatedAtUtcInvalid.ErrorMessage);
    }

    public static TheoryData<Action<PlatformUser>> UpdateActionsWithDefaultDate => new()
    {
        user => user.UpdateProfile(FirstName.Create("Jane"), LastName.Create("Smith"), default),
        user => user.ChangeEmail(UserEmail.Create("jane.smith@test.com"), default),
        user => user.ChangePasswordHash(PasswordHash.Create("new-hashed-password"), default),
        user => user.Deactivate(default),
        user => user.Activate(default)
    };

    public static TheoryData<Action<PlatformUser>> UpdateActionsWithInvalidDate => new()
    {
        user => user.UpdateProfile(FirstName.Create("Jane"), LastName.Create("Smith"), CreatedAtUtc.AddTicks(-1)),
        user => user.ChangeEmail(UserEmail.Create("jane.smith@test.com"), CreatedAtUtc.AddTicks(-1)),
        user => user.ChangePasswordHash(PasswordHash.Create("new-hashed-password"), CreatedAtUtc.AddTicks(-1)),
        user => user.Deactivate(CreatedAtUtc.AddTicks(-1)),
        user => user.Activate(CreatedAtUtc.AddTicks(-1))
    };

    private static PlatformUser CreateUser(
        UserEmail? email = null,
        PasswordHash? passwordHash = null,
        FirstName? firstName = null,
        LastName? lastName = null)
    {
        return PlatformUser.Create(
            UserId,
            email ?? UserEmail.Create("john.doe@test.com"),
            passwordHash ?? PasswordHash.Create("hashed-password"),
            firstName ?? FirstName.Create("John"),
            lastName ?? LastName.Create("Doe"),
            CreatedAtUtc);
    }
}
