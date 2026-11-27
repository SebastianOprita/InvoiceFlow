using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class AddPaymentAllocationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid Actor = Guid.CreateVersion7();
    private static readonly RequestHash Hash = RequestHash.Create(new string('A', 64));

    private readonly Mock<IPaymentsRepository> _payments = new();
    private readonly Mock<IIdempotencyRecordsRepository> _idempotency = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly AddPaymentAllocationCommandHandler _sut;

    public AddPaymentAllocationCommandHandlerTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(Now.AddHours(2));
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync<PaymentDto>(
                It.IsAny<Func<CancellationToken, Task<Result<PaymentDto>>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Data.IsolationLevel>()))
            .Returns((Func<CancellationToken, Task<Result<PaymentDto>>> action, CancellationToken ct, System.Data.IsolationLevel _) => action(ct));

        _sut = new(
            _payments.Object,
            _idempotency.Object,
            new FixedHasher(Hash),
            _unitOfWork.Object,
            _timeProvider.Object);
    }

    [Fact]
    public async Task Handle_AddsSpecifiedPartialAllocationAndSaves()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment, 75m);
        var command = Command(payment, balance, 40m);
        SetupNoIdempotencyRecord(command);
        _payments.Setup(x => x.GetPaymentWithLockAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _payments.Setup(x => x.GetInvoicePaymentBalanceWithLockAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.AllocatedAmount.Amount.Should().Be(40m);
        payment.UnallocatedAmount.Amount.Should().Be(60m);
        balance.AllocatedAmount.Amount.Should().Be(40m);
        result.Value!.Allocations.Should().ContainSingle(allocation => allocation.Amount == 40m && allocation.InvoiceId == balance.InvoiceId);
        _idempotency.Verify(x => x.Add(It.Is<IdempotencyRecord>(record =>
            record.Operation == IdempotencyOperation.AddPaymentAllocation &&
            record.ResourceId == payment.Allocations.Single().Id)), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReplaysMatchingIdempotencyRecordWithItsAllocation()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment, 75m);
        var allocation = payment.AddAllocation(Guid.CreateVersion7(), balance, Money.Create(20m, payment.CurrencyCode), Actor, Now.AddHours(2));
        var command = Command(payment, balance, 20m);
        var record = IdempotencyRecord.Create(
            command.TenantId,
            IdempotencyOperation.AddPaymentAllocation,
            IdempotencyKey.Create(command.IdempotencyKey),
            Hash,
            allocation.Id,
            Now);
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.AddPaymentAllocation, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _payments.Setup(x => x.GetPaymentByIdWithAllocationsAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allocations.Should().ContainSingle(x => x.Id == allocation.Id);
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync<PaymentDto>(
            It.IsAny<Func<CancellationToken, Task<Result<PaymentDto>>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsKeyReused_WhenExistingKeyHasDifferentRequestHash()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment);
        var command = Command(payment, balance, 20m);
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.AddPaymentAllocation, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyRecord.Create(
                command.TenantId,
                IdempotencyOperation.AddPaymentAllocation,
                IdempotencyKey.Create(command.IdempotencyKey),
                RequestHash.Create(new string('B', 64)),
                Guid.CreateVersion7(),
                Now));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.IdempotencyKeyReused);
        _payments.VerifyNoOtherCalls();
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync<PaymentDto>(
            It.IsAny<Func<CancellationToken, Task<Result<PaymentDto>>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsResourceNotFound_WhenReplayRecordReferencesMissingAllocation()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment);
        var command = Command(payment, balance, 20m);
        var record = IdempotencyRecord.Create(
            command.TenantId,
            IdempotencyOperation.AddPaymentAllocation,
            IdempotencyKey.Create(command.IdempotencyKey),
            Hash,
            Guid.CreateVersion7(),
            Now);
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.AddPaymentAllocation, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _payments.Setup(x => x.GetPaymentByIdWithAllocationsAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.IdempotencyResourceNotFound);
    }

    [Fact]
    public async Task Handle_ReturnsPaymentNotFound_WhenPaymentDoesNotExist()
    {
        var command = Command();
        SetupNoIdempotencyRecord(command);
        _payments.Setup(x => x.GetPaymentWithLockAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentNotFound);
        _payments.Verify(x => x.GetInvoicePaymentBalanceWithLockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsPaymentMustBePosted_WhenPaymentIsDraft()
    {
        var payment = DraftPayment();
        var command = Command(payment, InvoiceBalance(payment), 20m);
        SetupNoIdempotencyRecord(command);
        _payments.Setup(x => x.GetPaymentWithLockAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentMustBePosted);
        _payments.Verify(x => x.GetInvoicePaymentBalanceWithLockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsInvoiceBalanceNotFound_WhenBalanceDoesNotExist()
    {
        var payment = PostedPayment();
        var command = Command(payment, InvoiceBalance(payment), 20m);
        SetupNoIdempotencyRecord(command);
        _payments.Setup(x => x.GetPaymentWithLockAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _payments.Setup(x => x.GetInvoicePaymentBalanceWithLockAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoicePaymentBalance?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoicePaymentBalanceNotFound);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsCurrencyMismatch_WhenPaymentAndInvoiceCurrenciesDiffer()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment, currency: "USD");
        var command = Command(payment, balance, 20m);
        SetupNoIdempotencyRecord(command);
        SetupPaymentAndBalance(payment, balance, command);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentCurrencyMismatch);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsPrecisionError_WhenAmountExceedsCurrencyPrecision()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment);
        var command = Command(payment, balance, 10.001m);
        SetupNoIdempotencyRecord(command);
        SetupPaymentAndBalance(payment, balance, command);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.AllocationAmountPrecisionInvalid);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsAmountExceedsOutstanding_WhenAllocationIsGreaterThanInvoiceBalance()
    {
        var payment = PostedPayment();
        var balance = InvoiceBalance(payment, 30m);
        var command = Command(payment, balance, 40m);
        SetupNoIdempotencyRecord(command);
        SetupPaymentAndBalance(payment, balance, command);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.AllocationAmountExceedsInvoiceOutstanding);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsInsufficientPaymentAmount_WhenAllocationExceedsUnallocatedAmount()
    {
        var payment = PostedPayment(30m);
        var balance = InvoiceBalance(payment, 50m);
        var command = Command(payment, balance, 40m);
        SetupNoIdempotencyRecord(command);
        SetupPaymentAndBalance(payment, balance, command);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InsufficientUnallocatedPaymentAmount);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupNoIdempotencyRecord(AddPaymentAllocationCommand command)
    {
        _idempotency.Setup(x => x.GetIdempotencyRecordAsync(
                command.TenantId,
                IdempotencyOperation.AddPaymentAllocation,
                It.IsAny<IdempotencyKey>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdempotencyRecord?)null);
    }

    private void SetupPaymentAndBalance(Payment payment, InvoicePaymentBalance balance, AddPaymentAllocationCommand command)
    {
        _payments.Setup(x => x.GetPaymentWithLockAsync(command.TenantId, command.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _payments.Setup(x => x.GetInvoicePaymentBalanceWithLockAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);
    }

    private static AddPaymentAllocationCommand Command() =>
        new("key-1", Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), 20m, Actor);

    private static AddPaymentAllocationCommand Command(Payment payment, InvoicePaymentBalance balance, decimal amount) =>
        new("key-1", payment.TenantId, payment.Id, balance.InvoiceId, amount, Actor);

    private static Payment DraftPayment(decimal amount = 100m, string currency = "RON")
    {
        var currencyCode = CurrencyCode.Create(currency);
        return Payment.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            currencyCode,
            Money.Create(amount, currencyCode),
            PaymentMethod.Card,
            null,
            null,
            Actor,
            Now);
    }

    private static Payment PostedPayment(decimal amount = 100m, string currency = "RON")
    {
        var payment = DraftPayment(amount, currency);
        payment.Post(PaymentNumber.Create("PAY-2025-000001"), new DateOnly(2025, 1, 2), Actor, Now.AddHours(1));
        return payment;
    }

    private static InvoicePaymentBalance InvoiceBalance(Payment payment, decimal amount = 100m, string? currency = null)
    {
        var currencyCode = CurrencyCode.Create(currency ?? payment.CurrencyCode.Value);
        return InvoicePaymentBalance.Create(
            payment.TenantId,
            Guid.CreateVersion7(),
            payment.CustomerId,
            InvoiceNumber.Create("INV-2025-000001"),
            currencyCode,
            Money.Create(amount, currencyCode),
            Now);
    }

    private sealed class FixedHasher(RequestHash hash) : IRequestHasher
    {
        public Result<RequestHash> Hash<TRequest>(TRequest request) => Result<RequestHash>.Success(hash);
    }
}
