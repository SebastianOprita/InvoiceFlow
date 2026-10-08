using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers;

public sealed class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomersRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _mockRepository = new();
        _mockUnitOfWork = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new (_mockUnitOfWork.Object, _mockRepository.Object, _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomerAndSave()
    {
        // Arrange
        var command = TestConstants.CreateCustomerCommand(TestConstants.TenantId);

        _mockRepository.Setup(x => x.AddCustomer(It.IsAny<Customer>()));

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeNull();
        _mockRepository.Verify(x => x.AddCustomer(It.IsAny<Customer>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var command = TestConstants.CreateCustomerCommand(TestConstants.TenantId);

        _mockRepository.Setup(x => x.ExistsByCodeAsync(It.IsAny<Guid>(), CustomerCode.Create(command.CustomerCode)))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.CustomerCodeAlreadyExists.Code);
        result.Error.Message.Should().Be(ApplicationErrors.CustomerCodeAlreadyExists.Message);
        _mockRepository.Verify(x => x.AddCustomer(It.IsAny<Customer>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateRegistrationNumber_ShouldReturnFailure()
    {
        // Arrange
        var command = TestConstants.CreateCustomerCommand(TestConstants.TenantId);

        _mockRepository.Setup(x => x.ExistsByRegistrationNumberAsync(It.IsAny<Guid>(), RegistrationNumber.Create(command.CustomerTaxDetails.RegistrationNumber)))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.CustomerRegistrationNumberAlreadyExists.Code);
        result.Error.Message.Should().Be(ApplicationErrors.CustomerRegistrationNumberAlreadyExists.Message);
        _mockRepository.Verify(x => x.AddCustomer(It.IsAny<Customer>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateTaxNumber_ShouldReturnFailure()
    {
        // Arrange
        var command = TestConstants.CreateCustomerCommand(TestConstants.TenantId);

        _mockRepository.Setup(x => x.ExistsByTaxNumberAsync(It.IsAny<Guid>(), TaxNumber.Create(command.CustomerTaxDetails.TaxNumber!)))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.CustomerTaxNumberAlreadyExists.Code);
        result.Error.Message.Should().Be(ApplicationErrors.CustomerTaxNumberAlreadyExists.Message);
        _mockRepository.Verify(x => x.AddCustomer(It.IsAny<Customer>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
