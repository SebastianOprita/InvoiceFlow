using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class PostPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 2, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IPaymentsRepository> _payments = new();
    private readonly Mock<IPaymentNumbersRepository> _numbers = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly PostPaymentCommandHandler _sut;

    public PostPaymentCommandHandlerTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(Now);
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<Result>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Data.IsolationLevel>()))
            .Returns((Func<CancellationToken, Task<Result>> action, CancellationToken ct, System.Data.IsolationLevel _) => action(ct));

        _sut = new(_payments.Object, _numbers.Object, _unitOfWork.Object, _timeProvider.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        var cmd = Command();
        _payments.Setup(x => x.GetPaymentWithLockAsync(cmd.TenantId, cmd.PaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentNotFound);
        _numbers.Verify(x => x.AllocateNextWithLockAsync(It.IsAny<Guid>(), It.IsAny<FiscalYear>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessWithoutSaving_WhenAlreadyPosted()
    {
        var payment = PaymentMapperTests.CreatePayment();
        payment.Post(PaymentNumber.Create("PAY-2025-000001"), new DateOnly(2025, 1, 2), Guid.CreateVersion7(), Now);
        var cmd = new PostPaymentCommand(payment.TenantId, payment.Id, Guid.CreateVersion7());
        _payments.Setup(x => x.GetPaymentWithLockAsync(cmd.TenantId, cmd.PaymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _numbers.Verify(x => x.AllocateNextWithLockAsync(It.IsAny<Guid>(), It.IsAny<FiscalYear>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PostsDraftAndSaves()
    {
        var payment = PaymentMapperTests.CreatePayment();
        var cmd = new PostPaymentCommand(payment.TenantId, payment.Id, Guid.CreateVersion7());
        _payments.Setup(x => x.GetPaymentWithLockAsync(cmd.TenantId, cmd.PaymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _numbers.Setup(x => x.AllocateNextWithLockAsync(cmd.TenantId, FiscalYear.Create(2025), It.IsAny<CancellationToken>())).ReturnsAsync("PAY-2025-000001");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentLifecycleStatus.Posted);
        payment.Number!.Value.Should().Be("PAY-2025-000001");
        payment.PaymentDate.Should().Be(new DateOnly(2025, 1, 2));
        payment.PostedBy.Should().Be(cmd.PostedBy);
        payment.PostedAtUtc.Should().Be(Now);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessAfterConcurrencyConflict_WhenAnotherRequestPostedPayment()
    {
        var payment = PaymentMapperTests.CreatePayment();
        var postedPayment = CreatePayment(payment.TenantId, payment.Id);
        postedPayment.Post(PaymentNumber.Create("PAY-2025-000001"), new DateOnly(2025, 1, 2), Guid.CreateVersion7(), Now);
        var cmd = new PostPaymentCommand(payment.TenantId, payment.Id, Guid.CreateVersion7());
        _payments.Setup(x => x.GetPaymentWithLockAsync(cmd.TenantId, cmd.PaymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _payments.Setup(x => x.GetPaymentByIdAsync(cmd.TenantId, cmd.PaymentId, It.IsAny<CancellationToken>())).ReturnsAsync(postedPayment);
        _numbers.Setup(x => x.AllocateNextWithLockAsync(cmd.TenantId, FiscalYear.Create(2025), It.IsAny<CancellationToken>())).ReturnsAsync("PAY-2025-000002");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure(ApplicationErrors.ConcurrencyConflict));

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.ClearTrackedChanges(), Times.Once);
    }

    private static PostPaymentCommand Command() => new(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());

    private static Payment CreatePayment(Guid tenantId, Guid paymentId) => Payment.Create(
        tenantId,
        paymentId,
        Guid.CreateVersion7(),
        CurrencyCode.Create("RON"),
        Money.Create(25m, CurrencyCode.Create("RON")),
        PaymentMethod.Card,
        PaymentReference.Create("INVEST-1"),
        PaymentNotes.Create("test note"),
        Guid.CreateVersion7(),
        DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
}
