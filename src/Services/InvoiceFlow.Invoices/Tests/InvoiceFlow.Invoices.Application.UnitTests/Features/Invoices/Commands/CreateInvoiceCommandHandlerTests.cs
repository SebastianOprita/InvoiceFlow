using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoices = new();
    private readonly Mock<IIdempotencyRepository> _idempotency = new();
    private readonly Mock<ICustomersService> _customers = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISystemDateTimeProvider> _clock = new();
    private readonly IRequestHasher _hasher = new FixedRequestHasher();
    private readonly CreateInvoiceCommandHandler _sut;

    public CreateInvoiceCommandHandlerTests()
    {
        _clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync<InvoiceDto>(It.IsAny<Func<CancellationToken, Task<Result<InvoiceDto>>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()))
            .Returns((Func<CancellationToken, Task<Result<InvoiceDto>>> action, CancellationToken ct, System.Data.IsolationLevel _) => action(ct));
        _sut = new(_invoices.Object, _idempotency.Object, _customers.Object, _hasher, _unitOfWork.Object, _clock.Object);
    }

    [Fact]
    public async Task Handle_CreatesAndSavesInvoice_WhenNoIdempotencyRecordExists()
    {
        var cmd = new CreateInvoiceCommand("key-1", InvoiceTestData.TenantId, "CUST-001", "notes");
        _customers.Setup(x => x.GetCustomerAsync(cmd.TenantId, cmd.CustomerCode, It.IsAny<CancellationToken>())).ReturnsAsync(Result<InvoiceCustomerSnapshot>.Success(InvoiceTestData.Customer()));
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(cmd.TenantId, IdempotencyOperation.CreateInvoice, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _invoices.Verify(x => x.AddInvoice(It.Is<Invoice>(i => i.TenantId == cmd.TenantId)), Times.Once);
        _idempotency.Verify(x => x.Add(It.Is<IdempotencyRecord>(r => r.Operation == IdempotencyOperation.CreateInvoice)), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReplaysExistingInvoice_WhenMatchingKeyExists()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var cmd = new CreateInvoiceCommand("key-1", invoice.TenantId, "CUST-001", "notes");
        var record = IdempotencyRecord.Create(cmd.TenantId, IdempotencyOperation.CreateInvoice, IdempotencyKey.Create(cmd.IdempotencyKey), RequestHash.Create(new string('A', 64)), invoice.Id, _clock.Object.GetUtcNow());
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(cmd.TenantId, IdempotencyOperation.CreateInvoice, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _invoices.Setup(x => x.GetInvoiceByIdAsync(cmd.TenantId, invoice.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync<InvoiceDto>(It.IsAny<Func<CancellationToken, Task<Result<InvoiceDto>>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsKeyReused_WhenExistingKeyHasDifferentRequestHash()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var cmd = new CreateInvoiceCommand("key-1", invoice.TenantId, "CUST-001", "different notes");
        var record = IdempotencyRecord.Create(cmd.TenantId, IdempotencyOperation.CreateInvoice, IdempotencyKey.Create(cmd.IdempotencyKey), RequestHash.Create(new string('B', 64)), invoice.Id, _clock.Object.GetUtcNow());
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(cmd.TenantId, IdempotencyOperation.CreateInvoice, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.IdempotencyKeyReused);
    }

    private sealed class FixedRequestHasher : IRequestHasher
    {
        public Result<RequestHash> Hash<TRequest>(TRequest request) => Result<RequestHash>.Success(RequestHash.Create(new string('A', 64)));
    }
}
