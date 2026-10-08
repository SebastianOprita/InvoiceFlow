using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers;

public sealed class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomersRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly UpdateCustomerCommandHandler _handler;

    private static Guid CustomerId = Guid.CreateVersion7();

    public UpdateCustomerCommandHandlerTests()
    {
        _mockRepository = new Mock<ICustomersRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDateTimeProvider = new Mock<ISystemDateTimeProvider>();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new (_mockUnitOfWork.Object, _mockRepository.Object, _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var command = TestConstants.UpdateCustomerCommand(TestConstants.TenantId, CustomerId);

        _mockRepository
            .Setup(x => x.GetTrackedCustomerByIdAsync(command.TenantId, command.CustomerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.CustomerNotFound.Code);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCustomerAndReturnDto_WhenCommandIsValid()
    {
        // Arrange
        var customer = TestConstants.Customer(TestConstants.TenantId, CustomerId, "Existing customer", _mockDateTimeProvider.Object.Now);
        var command = TestConstants.UpdateCustomerCommand(TestConstants.TenantId, CustomerId, "Updated customer");

        _mockRepository
            .Setup(x => x.GetTrackedCustomerByIdAsync(command.TenantId, command.CustomerId))
            .ReturnsAsync(customer);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Id.Should().Be(customer.Id);
        result.Value.Name.Should().Be("Updated customer");

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldOnlyUpdateProvidedFields_WhenCommandIsPartial()
    {
        // Arrange
        var customer = TestConstants.Customer(TestConstants.TenantId, CustomerId, "Existing customer", _mockDateTimeProvider.Object.Now);
        var originalDto = customer.ToDto();

        var command = TestConstants.UpdateCustomerCommand(TestConstants.TenantId, CustomerId, "Only name updated") with
        {
            CustomerContact = null,
            CustomerAddress = null,
            CustomerCreditPolicy = null
        };

        _mockRepository
            .Setup(x => x.GetTrackedCustomerByIdAsync(command.TenantId, command.CustomerId))
            .ReturnsAsync(customer);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Name.Should().Be("Only name updated");

        result.Value.CustomerContact.Should().BeEquivalentTo(originalDto.CustomerContact);
        result.Value.CustomerAddress.Should().BeEquivalentTo(originalDto.CustomerAddress);
        result.Value.CustomerCreditPolicy.Should().BeEquivalentTo(originalDto.CustomerCreditPolicy);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
