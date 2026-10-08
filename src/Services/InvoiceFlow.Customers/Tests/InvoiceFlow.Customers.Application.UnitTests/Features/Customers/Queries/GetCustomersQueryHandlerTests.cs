using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using Moq;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers.Queries;

public sealed class GetCustomersQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Guid TenantId = Guid.CreateVersion7();
    private readonly Mock<ICustomersRepository> _mockRepository;
    private readonly GetCustomersQueryHandler _handler;

    public GetCustomersQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _mockRepository = new();
        _handler = new GetCustomersQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnCustomerDto()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var name = "CustomerName";
        var customer = TestConstants.Customer(TenantId, customerId, name, _mockDateTimeProvider.Object.Now);

        _mockRepository.Setup(x => x.GetAllCustomersAsync(TenantId))
            .ReturnsAsync([customer]);

        var query = new GetCustomersQuery(TenantId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(customerId);
        result.Value[0].Name.Should().Be(name);
        _mockRepository.Verify(x => x.GetAllCustomersAsync(TenantId), Times.Once);
    }
}
