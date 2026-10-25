using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepository = new();
    private readonly Mock<IIdempotencyRepository> _idempotencyRepository = new();
    private readonly Mock<ICustomerService> _customerService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISystemDateTimeProvider> _dateTimeProvider = new();
    private readonly CreateInvoiceCommandHandler _handler;
    private readonly RequestHash _hash = RequestHash.Create(new string('A', RequestHash.Sha256HexLength));

    public CreateInvoiceCommandHandlerTests()
    {
        _handler = new(_invoiceRepository.Object, _idempotencyRepository.Object, _customerService.Object,
            new FixedRequestHasher(_hash), _unitOfWork.Object, _dateTimeProvider.Object);
        _dateTimeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateInvoiceAndIdempotencyRecordAndSave()
    {
        var command = new CreateInvoiceCommand("key-123", InvoiceTestData.TenantId, "CUST-001", "Notes");
        _idempotencyRepository.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.CreateInvoice,
            It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        _customerService.Setup(x => x.GetCustomerAsync(command.TenantId, command.CustomerCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InvoiceCustomerSnapshot>.Success(InvoiceTestData.Customer()));
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<Result<InvoiceDto>>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<Result<InvoiceDto>>> operation, CancellationToken ct) => operation(ct));
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CustomerCode.Should().Be("CUST-001");
        _invoiceRepository.Verify(x => x.AddInvoice(It.IsAny<Invoice>()), Times.Once);
        _idempotencyRepository.Verify(x => x.Add(It.IsAny<IdempotencyRecord>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCustomerLookupFails_ShouldReturnFailureWithoutSaving()
    {
        var command = new CreateInvoiceCommand("key-123", InvoiceTestData.TenantId, "CUST-001", "Notes");
        _idempotencyRepository.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.CreateInvoice,
            It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        _customerService.Setup(x => x.GetCustomerAsync(command.TenantId, command.CustomerCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InvoiceCustomerSnapshot>.Failure(ApplicationErrors.CustomerNotFound));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ApplicationErrors.CustomerNotFound.Code);
        _invoiceRepository.Verify(x => x.AddInvoice(It.IsAny<Invoice>()), Times.Never);
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<Result<InvoiceDto>>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestHashFails_ShouldReturnFailureBeforeRepositoryCalls()
    {
        var handler = new CreateInvoiceCommandHandler(_invoiceRepository.Object, _idempotencyRepository.Object,
            _customerService.Object, new FixedRequestHasher(Result<RequestHash>.Failure(ApplicationErrors.RequestHashInputRequired)),
            _unitOfWork.Object, _dateTimeProvider.Object);
        var result = await handler.Handle(new("key-123", InvoiceTestData.TenantId, "CUST-001", "Notes"), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ApplicationErrors.RequestHashInputRequired.Code);
        _idempotencyRepository.Verify(x => x.GetIdempotencyRecordAsync(It.IsAny<Guid>(), It.IsAny<IdempotencyOperation>(),
            It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenIdempotencyKeyIsReusedForDifferentRequest_ShouldReturnConflict()
    {
        var command = new CreateInvoiceCommand("key-123", InvoiceTestData.TenantId, "CUST-001", "Notes");
        var record = IdempotencyRecord.Create(command.TenantId, IdempotencyOperation.CreateInvoice,
            IdempotencyKey.Create(command.IdempotencyKey), RequestHash.Create(new string('B', RequestHash.Sha256HexLength)),
            Guid.CreateVersion7(), new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _idempotencyRepository.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.CreateInvoice,
            It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ApplicationErrors.IdempotencyKeyReused.Code);
        _invoiceRepository.Verify(x => x.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenIdempotencyReplayResourceIsMissing_ShouldReturnConflict()
    {
        var command = new CreateInvoiceCommand("key-123", InvoiceTestData.TenantId, "CUST-001", "Notes");
        var record = IdempotencyRecord.Create(command.TenantId, IdempotencyOperation.CreateInvoice,
            IdempotencyKey.Create(command.IdempotencyKey), _hash, Guid.CreateVersion7(), new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _idempotencyRepository.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.CreateInvoice,
            It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _invoiceRepository.Setup(x => x.GetInvoiceByIdAsync(command.TenantId, record.ResourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ApplicationErrors.IdempotencyResourceNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenIdempotencyReplayResourceExists_ShouldReturnExistingInvoice()
    {
        var command = new CreateInvoiceCommand("key-123", InvoiceTestData.TenantId, "CUST-001", "Notes");
        var invoice = InvoiceTestData.DraftInvoice();
        var record = IdempotencyRecord.Create(command.TenantId, IdempotencyOperation.CreateInvoice,
            IdempotencyKey.Create(command.IdempotencyKey), _hash, invoice.Id, new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _idempotencyRepository.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.CreateInvoice,
            It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _invoiceRepository.Setup(x => x.GetInvoiceByIdAsync(command.TenantId, invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(invoice.Id);
        _customerService.Verify(x => x.GetCustomerAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class FixedRequestHasher(Result<RequestHash> result) : IRequestHasher
    {
        public FixedRequestHasher(RequestHash hash) : this(Result<RequestHash>.Success(hash)) { }
        public Result<RequestHash> Hash<TRequest>(TRequest request) => result;
    }
}
