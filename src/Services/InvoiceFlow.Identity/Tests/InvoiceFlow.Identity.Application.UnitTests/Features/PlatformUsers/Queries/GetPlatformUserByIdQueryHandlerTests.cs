using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class GetPlatformUserByIdQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IPlatformUsersRepository> _usersRepositoryMock = new();
    private readonly GetPlatformUserByIdQueryHandler _handler;

    public GetPlatformUserByIdQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new GetPlatformUserByIdQueryHandler(_usersRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserExists_ShouldReturnSuccessResultWithUserDto()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        var user = PlatformUser.Create
        (
            userId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        var query = new GetPlatformUserByIdQuery(userId);

        _usersRepositoryMock
            .Setup(x => x.FindUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(user.ToDto());

        _usersRepositoryMock.Verify(
            x => x.FindUserByIdAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        var query = new GetPlatformUserByIdQuery(userId);

        _usersRepositoryMock
            .Setup(x => x.FindUserByIdAsync(userId))
            .ReturnsAsync((PlatformUser?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().BeEquivalentTo(new ApplicationError(
            ApplicationErrorType.Unauthorized,
            ApplicationErrors.UserUnauthorized.Code,
            ApplicationErrors.UserUnauthorized.Message));

        _usersRepositoryMock.Verify(
            x => x.FindUserByIdAsync(userId),
            Times.Once);
    }
}
