using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class AddInvoiceLineCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoices = new();
    private readonly Mock<IIdempotencyRepository> _idempotency = new();
    private readonly Mock<IProductsService> _products = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISystemDateTimeProvider> _clock = new();
    private readonly IRequestHasher _hasher = new FixedRequestHasher();
    private readonly AddInvoiceLineCommandHandler _sut;

    public AddInvoiceLineCommandHandlerTests()
    {
        _clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync<InvoiceDto>(It.IsAny<Func<CancellationToken, Task<Result<InvoiceDto>>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()))
            .Returns((Func<CancellationToken, Task<Result<InvoiceDto>>> action, CancellationToken ct, System.Data.IsolationLevel _) => action(ct));
        _sut = new(_invoices.Object, _idempotency.Object, _products.Object, _hasher, _unitOfWork.Object, _clock.Object);
    }

    [Fact]
    public async Task Handle_AddsProductLineAndSaves_WhenNoIdempotencyRecordExists()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var cmd = new AddInvoiceLineCommand("key-1", invoice.TenantId, invoice.Id, "PROD-001", 2m, 5m);
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(cmd.TenantId, IdempotencyOperation.AddInvoiceLine, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        _products.Setup(x => x.GetProductAsync(cmd.TenantId, cmd.ProductCode, It.IsAny<CancellationToken>())).ReturnsAsync(Result<InvoiceProductSnapshot>.Success(Product()));
        _invoices.Setup(x => x.GetTrackedInvoiceByIdAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Lines.Should().HaveCount(2);
        _idempotency.Verify(x => x.Add(It.Is<IdempotencyRecord>(r => r.Operation == IdempotencyOperation.AddInvoiceLine && r.ResourceId == invoice.Lines.Last().Id)), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReplaysExistingInvoice_WhenMatchingKeyExists()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var cmd = new AddInvoiceLineCommand("key-1", invoice.TenantId, invoice.Id, "PROD-001", 2m, null);
        var record = IdempotencyRecord.Create(cmd.TenantId, IdempotencyOperation.AddInvoiceLine, IdempotencyKey.Create(cmd.IdempotencyKey), RequestHash.Create(new string('A', 64)), Guid.CreateVersion7(), _clock.Object.GetUtcNow());
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(cmd.TenantId, IdempotencyOperation.AddInvoiceLine, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _invoices.Setup(x => x.GetInvoiceByIdAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync<InvoiceDto>(It.IsAny<Func<CancellationToken, Task<Result<InvoiceDto>>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsInvoiceNotFound_WhenInvoiceDoesNotExist()
    {
        var cmd = new AddInvoiceLineCommand("key-1", InvoiceTestData.TenantId, Guid.CreateVersion7(), "PROD-001", 2m, null);
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(cmd.TenantId, IdempotencyOperation.AddInvoiceLine, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        _products.Setup(x => x.GetProductAsync(cmd.TenantId, cmd.ProductCode, It.IsAny<CancellationToken>())).ReturnsAsync(Result<InvoiceProductSnapshot>.Success(Product()));
        _invoices.Setup(x => x.GetTrackedInvoiceByIdAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoiceNotFound);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static InvoiceProductSnapshot Product() => InvoiceProductSnapshot.Create(
        InvoiceTestData.TenantId, Guid.CreateVersion7(), true, ProductCode.Create("PROD-001"), ProductName.Create("Service"),
        ProductDescription.Create("Professional service"), UnitOfMeasure.Service, Money.Create(100, CurrencyCode.Create("RON")), Percentage.Create(19));

    private sealed class FixedRequestHasher : IRequestHasher
    {
        public Result<RequestHash> Hash<TRequest>(TRequest request) => Result<RequestHash>.Success(RequestHash.Create(new string('A', 64)));
    }
}
