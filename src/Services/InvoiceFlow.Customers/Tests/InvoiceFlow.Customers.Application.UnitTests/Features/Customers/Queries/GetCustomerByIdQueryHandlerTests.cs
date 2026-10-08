using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers.Queries;

public sealed class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Guid TenantId = Guid.CreateVersion7();
    private readonly Mock<ICustomersRepository> _mockRepository;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _mockRepository = new();
        _handler = new GetCustomerByIdQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnCustomerDto()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var name = "CustomerName";
        var customer = TestConstants.Customer(TenantId, customerId, name, _mockDateTimeProvider.Object.Now);

        _mockRepository.Setup(x => x.GetCustomerByIdAsync(TenantId, customerId, CancellationToken.None))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery(TenantId, customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(customerId);
        result.Value.Name.Should().Be(name);
        _mockRepository.Verify(x => x.GetCustomerByIdAsync(TenantId, customerId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();

        _mockRepository.Setup(x => x.GetCustomerByIdAsync(TenantId, customerId, CancellationToken.None))
            .ReturnsAsync((Customer)null!);

        var query = new GetCustomerByIdQuery(TenantId, customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
    }
}
