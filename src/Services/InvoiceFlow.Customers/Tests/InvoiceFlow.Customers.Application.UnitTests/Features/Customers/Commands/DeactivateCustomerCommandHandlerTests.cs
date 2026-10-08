using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers;

public sealed class DeactivateCustomerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICustomersRepository> _mockRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly DeactivateCustomerCommandHandler _handler;

    public DeactivateCustomerCommandHandlerTests()
    {
        _mockRepository = new Mock<ICustomersRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDateTimeProvider = new Mock<ISystemDateTimeProvider>();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new(_mockUnitOfWork.Object, _mockRepository.Object, _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var cmd = new DeactivateCustomerCommand(
            TenantId: Guid.CreateVersion7(),
            CustomerId: Guid.CreateVersion7());

        _mockRepository
            .Setup(x => x.GetTrackedCustomerByIdAsync(cmd.TenantId, cmd.CustomerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.CustomerNotFound.Code);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithoutSaving_WhenCustomerIsAlreadyDeactive()
    {
        // Arrange
        var customer = TestConstants.Customer(TestConstants.TenantId, Guid.CreateVersion7(), "Existing customer", _mockDateTimeProvider.Object.Now);

        if (customer.IsActive)
            customer.Deactivate(_mockDateTimeProvider.Object.Now);

        var cmd = new DeactivateCustomerCommand(
            TenantId: customer.TenantId,
            CustomerId: customer.Id);

        _mockRepository
            .Setup(x => x.GetTrackedCustomerByIdAsync(cmd.TenantId, cmd.CustomerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateCustomerAndSave_WhenCustomerIsActive()
    {
        // Arrange
        var customer = TestConstants.Customer(TestConstants.TenantId, Guid.CreateVersion7(), "Existing customer", _mockDateTimeProvider.Object.Now);

        if (!customer.IsActive)
            customer.Activate(_mockDateTimeProvider.Object.Now);

        var cmd = new DeactivateCustomerCommand(
            TenantId: customer.TenantId,
            CustomerId: customer.Id);

        _mockRepository
            .Setup(x => x.GetTrackedCustomerByIdAsync(cmd.TenantId, cmd.CustomerId))
            .ReturnsAsync(customer);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
