using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public sealed class GetPlatformUsersQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly GetUsersQueryHandler _sut;

    public GetPlatformUsersQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _usersRepositoryMock = new();
        _sut = new GetUsersQueryHandler(_usersRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ShouldReturnMappedUserDtos()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var users = new List<User>
        {
            User.Create(
                tenantId,
                Guid.CreateVersion7(),
                UserEmail.Create("john.doe@test.com"),
                PasswordHash.Create("hashedpassword"),
                FirstName.Create("John"),
                LastName.Create("Doe"),
                _mockDateTimeProvider.Object.Now),
            User.Create(
                tenantId,
                Guid.CreateVersion7(),
                UserEmail.Create("jane.doe@test.com"),
                PasswordHash.Create("hashedpassword"),
                FirstName.Create("Jane"),
                LastName.Create("Doe"),
                _mockDateTimeProvider.Object.Now)
        };

        var query = new GetUsersQuery(tenantId);

        _usersRepositoryMock
            .Setup(x => x.FindAllUsersAsync(tenantId))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(
            users.Select(u => u.ToDto()));

        _usersRepositoryMock.Verify(
            x => x.FindAllUsersAsync(tenantId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var query = new GetUsersQuery(tenantId);

        _usersRepositoryMock
            .Setup(x => x.FindAllUsersAsync(tenantId))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _usersRepositoryMock.Verify(
            x => x.FindAllUsersAsync(tenantId),
            Times.Once);
    }
}
