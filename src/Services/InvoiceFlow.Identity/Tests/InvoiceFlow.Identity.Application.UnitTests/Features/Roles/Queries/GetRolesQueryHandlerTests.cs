using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public sealed class GetRolesQueryHandlerTests
{
    private readonly Mock<IRolesRepository> _rolesRepositoryMock;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly GetRolesQueryHandler _sut;

    public GetRolesQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _rolesRepositoryMock = new();
        _sut = new GetRolesQueryHandler(_rolesRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRolesExist_ShouldReturnMappedRoleDtos()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var roles = new List<Role>
        {
            Role.Create(
                tenantId,
                Guid.CreateVersion7(),
                RoleName.Create("Admin"),
                _mockDateTimeProvider.Object.Now),
            Role.Create(
                tenantId,
                Guid.CreateVersion7(),
                RoleName.Create("User"),
                _mockDateTimeProvider.Object.Now)
        };

        var query = new GetRolesQuery(tenantId);

        _rolesRepositoryMock
            .Setup(x => x.FindAllRolesAsync(tenantId))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(
            roles.Select(r => r.ToDto()));

        _rolesRepositoryMock.Verify(
            x => x.FindAllRolesAsync(tenantId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoRolesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var query = new GetRolesQuery(tenantId);

        _rolesRepositoryMock
            .Setup(x => x.FindAllRolesAsync(tenantId))
            .ReturnsAsync(new List<Role>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _rolesRepositoryMock.Verify(
            x => x.FindAllRolesAsync(tenantId),
            Times.Once);
    }
}
