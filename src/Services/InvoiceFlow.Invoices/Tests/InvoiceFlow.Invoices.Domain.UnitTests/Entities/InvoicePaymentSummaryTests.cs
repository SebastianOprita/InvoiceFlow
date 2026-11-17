using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Entities;

public sealed class InvoicePaymentSummaryTests
{
    private static readonly Guid TenantId = Guid.CreateVersion7();
    private static readonly Guid InvoiceId = Guid.CreateVersion7();
    private static readonly CurrencyCode Ron = CurrencyCode.Create("RON");
    private static readonly DateTimeOffset SourceTime =
        new(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, 100, InvoicePaymentStatus.Unpaid)]
    [InlineData(25, 75, InvoicePaymentStatus.PartiallyPaid)]
    [InlineData(100, 0, InvoicePaymentStatus.Paid)]
    public void Create_DerivesStatusFromAmounts(
        decimal paidAmount,
        decimal outstandingAmount,
        InvoicePaymentStatus expectedStatus)
    {
        var summary = CreateSummary(paidAmount, outstandingAmount);

        summary.Status.Should().Be(expectedStatus);
        summary.PaidAmount.Amount.Should().Be(paidAmount);
        summary.OutstandingAmount.Amount.Should().Be(outstandingAmount);
        summary.PaymentBalanceVersion.Should().Be(1);
    }

    [Fact]
    public void Create_NormalizesTimestampsToUtc()
    {
        var localOffset = TimeSpan.FromHours(3);
        var sourceTime = new DateTimeOffset(2025, 1, 1, 13, 0, 0, localOffset);
        var appliedTime = new DateTimeOffset(2025, 1, 1, 14, 0, 0, localOffset);

        var summary = InvoicePaymentSummary.Create(
            TenantId,
            InvoiceId,
            Money.Create(10m, Ron),
            Money.Create(90m, Ron),
            1,
            sourceTime,
            appliedTime);

        summary.SourceUpdatedAtUtc.Should().Be(sourceTime.ToUniversalTime());
        summary.LastAppliedAtUtc.Should().Be(appliedTime.ToUniversalTime());
    }

    [Fact]
    public void Create_CopiesAmounts_WhenSameMoneyInstanceIsPassedToBothOwnedValues()
    {
        var sharedAmount = Money.Create(50m, Ron);
        var summary = InvoicePaymentSummary.Create(
            TenantId,
            InvoiceId,
            sharedAmount,
            sharedAmount,
            1,
            SourceTime,
            SourceTime);

        ReferenceEquals(summary.PaidAmount, summary.OutstandingAmount).Should().BeFalse();
        ReferenceEquals(summary.PaidAmount, sharedAmount).Should().BeFalse();
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        Action act = () => InvoicePaymentSummary.Create(
            Guid.Empty, InvoiceId, Money.Create(0m, Ron), Money.Create(100m, Ron),
            0, SourceTime, SourceTime);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyInvoiceId()
    {
        Action act = () => InvoicePaymentSummary.Create(
            TenantId, Guid.Empty, Money.Create(0m, Ron), Money.Create(100m, Ron),
            0, SourceTime, SourceTime);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Create_RejectsNegativeVersion()
    {
        Action act = () => InvoicePaymentSummary.Create(
            TenantId, InvoiceId, Money.Create(0m, Ron), Money.Create(100m, Ron),
            -1, SourceTime, SourceTime);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentBalanceVersionInvalid);
    }

    [Fact]
    public void Create_RejectsDifferentCurrencies()
    {
        Action act = () => InvoicePaymentSummary.Create(
            TenantId,
            InvoiceId,
            Money.Create(10m, Ron),
            Money.Create(90m, CurrencyCode.Create("EUR")),
            0,
            SourceTime,
            SourceTime);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_RejectsMissingTimestamps(bool missingSourceTime)
    {
        var sourceTime = missingSourceTime ? default : SourceTime;
        var appliedTime = missingSourceTime ? SourceTime : default;
        Action act = () => InvoicePaymentSummary.Create(
            TenantId, InvoiceId, Money.Create(0m, Ron), Money.Create(100m, Ron),
            0, sourceTime, appliedTime);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcRequired);
    }

    [Fact]
    public void ApplySnapshot_UpdatesAmountsStatusVersionAndTimestamps()
    {
        var summary = CreateSummary(10m, 90m);
        var sourceTime = SourceTime.AddMinutes(1);
        var appliedTime = SourceTime.AddMinutes(2);

        var applied = summary.ApplySnapshot(
            Money.Create(40m, Ron),
            Money.Create(60m, Ron),
            2,
            sourceTime,
            appliedTime);

        applied.Should().BeTrue();
        summary.PaidAmount.Amount.Should().Be(40m);
        summary.OutstandingAmount.Amount.Should().Be(60m);
        summary.Status.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        summary.PaymentBalanceVersion.Should().Be(2);
        summary.SourceUpdatedAtUtc.Should().Be(sourceTime);
        summary.LastAppliedAtUtc.Should().Be(appliedTime);
    }

    [Fact]
    public void ApplySnapshot_ReturnsFalseAndPreservesState_WhenVersionIsStaleOrDuplicate()
    {
        var summary = CreateSummary(10m, 90m);

        var staleResult = summary.ApplySnapshot(
            Money.Create(50m, Ron), Money.Create(50m, Ron), 0,
            SourceTime.AddMinutes(5), SourceTime.AddMinutes(5));
        var duplicateResult = summary.ApplySnapshot(
            Money.Create(50m, Ron), Money.Create(50m, Ron), 1,
            SourceTime.AddMinutes(5), SourceTime.AddMinutes(5));

        staleResult.Should().BeFalse();
        duplicateResult.Should().BeFalse();
        summary.PaidAmount.Amount.Should().Be(10m);
        summary.OutstandingAmount.Amount.Should().Be(90m);
        summary.Status.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        summary.PaymentBalanceVersion.Should().Be(1);
        summary.SourceUpdatedAtUtc.Should().Be(SourceTime);
    }

    [Fact]
    public void ApplySnapshot_RejectsCurrencyMismatch()
    {
        var summary = CreateSummary(10m, 90m);

        Action act = () => summary.ApplySnapshot(
            Money.Create(20m, CurrencyCode.Create("EUR")),
            Money.Create(80m, CurrencyCode.Create("EUR")),
            2,
            SourceTime.AddMinutes(1),
            SourceTime.AddMinutes(1));

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
    }

    [Theory]
    [InlineData(0, 100, InvoicePaymentStatus.Unpaid)]
    [InlineData(100, 0, InvoicePaymentStatus.Paid)]
    public void ApplySnapshot_DerivesStatusFromUpdatedAmounts(
        decimal paidAmount,
        decimal outstandingAmount,
        InvoicePaymentStatus expectedStatus)
    {
        var summary = CreateSummary(10m, 90m);

        var applied = summary.ApplySnapshot(
            Money.Create(paidAmount, Ron),
            Money.Create(outstandingAmount, Ron),
            2,
            SourceTime.AddMinutes(1),
            SourceTime.AddMinutes(1));

        applied.Should().BeTrue();
        summary.Status.Should().Be(expectedStatus);
        summary.PaidAmount.Amount.Should().Be(paidAmount);
        summary.OutstandingAmount.Amount.Should().Be(outstandingAmount);
    }

    [Fact]
    public void ApplySnapshot_RejectsSourceTimestampEarlierThanCurrentSnapshot()
    {
        var summary = CreateSummary(10m, 90m);

        Action act = () => summary.ApplySnapshot(
            Money.Create(20m, Ron),
            Money.Create(80m, Ron),
            2,
            SourceTime.AddTicks(-1),
            SourceTime.AddMinutes(1));

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentSummaryTimestampInvalid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplySnapshot_RejectsMissingTimestamps(bool missingSourceTime)
    {
        var summary = CreateSummary(10m, 90m);
        var sourceTime = missingSourceTime ? default : SourceTime.AddMinutes(1);
        var appliedTime = missingSourceTime ? SourceTime.AddMinutes(1) : default;

        Action act = () => summary.ApplySnapshot(
            Money.Create(20m, Ron),
            Money.Create(80m, Ron),
            2,
            sourceTime,
            appliedTime);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcRequired);
    }

    private static InvoicePaymentSummary CreateSummary(
        decimal paidAmount,
        decimal outstandingAmount)
    {
        return InvoicePaymentSummary.Create(
            TenantId,
            InvoiceId,
            Money.Create(paidAmount, Ron),
            Money.Create(outstandingAmount, Ron),
            paymentBalanceVersion: 1,
            sourceUpdatedAtUtc: SourceTime,
            appliedAtUtc: SourceTime);
    }
}
