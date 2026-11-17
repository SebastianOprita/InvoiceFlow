using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class UpsertInvoicePaymentSummaryCommandHandlerTests
{
    private static readonly DateTimeOffset SourceTime =
        new(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IInvoicesRepository> _invoices = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpsertInvoicePaymentSummaryCommandHandler _sut;

    public UpsertInvoicePaymentSummaryCommandHandlerTests()
    {
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<Result>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Data.IsolationLevel>()))
            .Returns((Func<CancellationToken, Task<Result>> operation,
                CancellationToken ct,
                System.Data.IsolationLevel _) => operation(ct));

        _sut = new(_invoices.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_CreatesSummary_WhenItDoesNotExist()
    {
        var command = CreateCommand(version: 1);
        InvoicePaymentSummary? addedSummary = null;
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync((InvoicePaymentSummary?)null);
        _invoices.Setup(x => x.GetPaymentSummaryWithLockAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync((InvoicePaymentSummary?)null);
        _invoices.Setup(x => x.AddPaymentSummary(It.IsAny<InvoicePaymentSummary>()))
            .Callback<InvoicePaymentSummary>(summary => addedSummary = summary);
        _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        addedSummary.Should().NotBeNull();
        addedSummary!.TenantId.Should().Be(command.TenantId);
        addedSummary.InvoiceId.Should().Be(command.InvoiceId);
        addedSummary.PaidAmount.Amount.Should().Be(command.PaidAmount);
        addedSummary.OutstandingAmount.Amount.Should().Be(command.OutstandingAmount);
        addedSummary.PaymentBalanceVersion.Should().Be(command.PaymentBalanceVersion);
        addedSummary.Status.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        _unitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesSummary_WhenProvidedVersionIsNewer()
    {
        var command = CreateCommand(version: 2, paidAmount: 40m, outstandingAmount: 60m);
        var summary = CreateSummary(command.TenantId, command.InvoiceId, 10m, 90m, version: 1);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(summary);
        _invoices.Setup(x => x.GetPaymentSummaryWithLockAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(summary);
        _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        summary.PaidAmount.Amount.Should().Be(40m);
        summary.OutstandingAmount.Amount.Should().Be(60m);
        summary.PaymentBalanceVersion.Should().Be(2);
        summary.Status.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        summary.SourceUpdatedAtUtc.Should().Be(command.SourceUpdatedAtUtc);
        _unitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessWithoutTransaction_WhenVersionAndSnapshotMatch()
    {
        var command = CreateCommand(version: 1);
        var summary = CreateSummary(
            command.TenantId,
            command.InvoiceId,
            command.PaidAmount,
            command.OutstandingAmount,
            version: command.PaymentBalanceVersion,
            sourceUpdatedAtUtc: command.SourceUpdatedAtUtc);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(summary);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task<Result>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsConflictWithoutTransaction_WhenVersionMatchesButSnapshotDiffers()
    {
        var command = CreateCommand(version: 1, paidAmount: 40m, outstandingAmount: 60m);
        var summary = CreateSummary(
            command.TenantId,
            command.InvoiceId,
            25m,
            75m,
            version: 1,
            sourceUpdatedAtUtc: command.SourceUpdatedAtUtc);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(summary);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentBalanceVersionConflict);
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task<Result>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<System.Data.IsolationLevel>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsStaleVersionErrorWithoutTransaction_WhenStoredVersionIsHigher()
    {
        var command = CreateCommand(version: 1);
        var summary = CreateSummary(command.TenantId, command.InvoiceId, 40m, 60m, version: 2);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(summary);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentBalanceVersionNotNewer);
        _unitOfWork.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task<Result>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessWithoutSaving_WhenLockedSummaryNowMatchesVersionAndSnapshot()
    {
        var command = CreateCommand(version: 2, paidAmount: 40m, outstandingAmount: 60m);
        var previouslyReadSummary = CreateSummary(command.TenantId, command.InvoiceId, 10m, 90m, version: 1);
        var lockedSummary = CreateSummary(
            command.TenantId,
            command.InvoiceId,
            command.PaidAmount,
            command.OutstandingAmount,
            command.PaymentBalanceVersion,
            command.SourceUpdatedAtUtc);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(previouslyReadSummary);
        _invoices.Setup(x => x.GetPaymentSummaryWithLockAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(lockedSummary);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenLockedSummaryHasSameVersionButDifferentSnapshot()
    {
        var command = CreateCommand(version: 2, paidAmount: 40m, outstandingAmount: 60m);
        var previouslyReadSummary = CreateSummary(command.TenantId, command.InvoiceId, 10m, 90m, version: 1);
        var lockedSummary = CreateSummary(
            command.TenantId,
            command.InvoiceId,
            25m,
            75m,
            command.PaymentBalanceVersion,
            command.SourceUpdatedAtUtc);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(previouslyReadSummary);
        _invoices.Setup(x => x.GetPaymentSummaryWithLockAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(lockedSummary);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentBalanceVersionConflict);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsStaleVersionError_WhenLockedSummaryAdvancedPastCommand()
    {
        var command = CreateCommand(version: 2);
        var previouslyReadSummary = CreateSummary(command.TenantId, command.InvoiceId, 10m, 90m, version: 1);
        var lockedSummary = CreateSummary(command.TenantId, command.InvoiceId, 40m, 60m, version: 3);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(previouslyReadSummary);
        _invoices.Setup(x => x.GetPaymentSummaryWithLockAsync(
                command.TenantId, command.InvoiceId, CancellationToken.None))
            .ReturnsAsync(lockedSummary);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.PaymentBalanceVersionNotNewer);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static UpsertInvoicePaymentSummaryCommand CreateCommand(
        long version,
        decimal paidAmount = 25m,
        decimal outstandingAmount = 75m)
    {
        return new UpsertInvoicePaymentSummaryCommand(
            InvoiceTestData.TenantId,
            Guid.CreateVersion7(),
            paidAmount,
            outstandingAmount,
            "RON",
            version,
            SourceTime.AddMinutes(version),
            SourceTime.AddMinutes(version));
    }

    private static InvoicePaymentSummary CreateSummary(
        Guid tenantId,
        Guid invoiceId,
        decimal paidAmount,
        decimal outstandingAmount,
        long version,
        DateTimeOffset? sourceUpdatedAtUtc = null)
    {
        return InvoicePaymentSummary.Create(
            tenantId,
            invoiceId,
            Money.Create(paidAmount, CurrencyCode.Create("RON")),
            Money.Create(outstandingAmount, CurrencyCode.Create("RON")),
            version,
            sourceUpdatedAtUtc ?? SourceTime,
            SourceTime);
    }
}
