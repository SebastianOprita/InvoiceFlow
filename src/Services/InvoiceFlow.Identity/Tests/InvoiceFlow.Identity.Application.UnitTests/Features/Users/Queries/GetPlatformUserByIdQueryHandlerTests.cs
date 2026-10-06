using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public sealed class GetPlatformUserByIdQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly GetUserByIdQueryHandler _sut;

    public GetPlatformUserByIdQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _usersRepositoryMock = new();
        _sut = new GetUserByIdQueryHandler(_usersRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccessResultWithUserDto()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var user = User.Create
        (
            tenantId,
            userId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        var query = new GetUserByIdQuery(tenantId, userId);

        _usersRepositoryMock
            .Setup(x => x.FindUserByIdAsync(tenantId, userId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(user.ToDto());

        _usersRepositoryMock.Verify(
            x => x.FindUserByIdAsync(tenantId, userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var query = new GetUserByIdQuery(tenantId, userId);

        _usersRepositoryMock
            .Setup(x => x.FindUserByIdAsync(tenantId, userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().BeEquivalentTo(new ApplicationError(
            ApplicationErrorType.NotFound,
            ApplicationErrors.UserNotFound.Code,
            ApplicationErrors.UserNotFound.Message));

        _usersRepositoryMock.Verify(
            x => x.FindUserByIdAsync(tenantId, userId),
            Times.Once);
    }
}
