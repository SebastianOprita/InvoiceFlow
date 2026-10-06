using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public sealed class GetPlatformUsersQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IPlatformUsersRepository> _usersRepositoryMock = new();
    private readonly GetPlatformUsersQueryHandler _handler;

    public GetPlatformUsersQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new GetPlatformUsersQueryHandler(_usersRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPlatformUsersExist_ShouldReturnMappedUserDtos()
    {
        // Arrange
        var users = new List<PlatformUser>
        {
            Domain.PlatformUser.Create(
                Guid.CreateVersion7(),
                UserEmail.Create("john.doe@test.com"),
                PasswordHash.Create("hashedpassword"),
                FirstName.Create("John"),
                LastName.Create("Doe"),
                _mockDateTimeProvider.Object.Now),
            Domain.PlatformUser.Create(
                Guid.CreateVersion7(),
                UserEmail.Create("jane.doe@test.com"),
                PasswordHash.Create("hashedpassword"),
                FirstName.Create("Jane"),
                LastName.Create("Doe"),
                _mockDateTimeProvider.Object.Now)
        };

        var query = new GetPlatformUsersQuery();

        _usersRepositoryMock
            .Setup(x => x.FindAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(
            users.Select(u => u.ToDto()));

        _usersRepositoryMock.Verify(
            x => x.FindAllUsersAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoPlatformUsersExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetPlatformUsersQuery();

        _usersRepositoryMock
            .Setup(x => x.FindAllUsersAsync())
            .ReturnsAsync(new List<PlatformUser>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _usersRepositoryMock.Verify(
            x => x.FindAllUsersAsync(),
            Times.Once);
    }
}
