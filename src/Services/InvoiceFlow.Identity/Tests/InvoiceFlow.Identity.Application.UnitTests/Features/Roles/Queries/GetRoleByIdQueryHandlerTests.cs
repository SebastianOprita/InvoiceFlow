using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public sealed class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<IRolesRepository> _rolesRepositoryMock;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly GetRoleByIdQueryHandler _sut;

    public GetRoleByIdQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _rolesRepositoryMock = new();
        _sut = new GetRoleByIdQueryHandler(_rolesRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_ShouldReturnSuccessResultWithRoleDto()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var role = Role.Create
        (
            tenantId,
            roleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now
        );

        var query = new GetRoleByIdQuery(tenantId, roleId);

        _rolesRepositoryMock
            .Setup(x => x.FindRoleByIdAsync(tenantId, roleId))
            .ReturnsAsync(role);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(role.ToDto());

        _rolesRepositoryMock.Verify(
            x => x.FindRoleByIdAsync(tenantId, roleId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var query = new GetRoleByIdQuery(tenantId, roleId);

        _rolesRepositoryMock
            .Setup(x => x.FindRoleByIdAsync(tenantId, roleId))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().BeEquivalentTo(new ApplicationError(
            ApplicationErrorType.NotFound,
            ApplicationErrors.RoleNotFound.Code,
            ApplicationErrors.RoleNotFound.Message));

        _rolesRepositoryMock.Verify(
            x => x.FindRoleByIdAsync(tenantId, roleId),
            Times.Once);
    }
}
