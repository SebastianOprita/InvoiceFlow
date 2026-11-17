using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal sealed class UpsertInvoicePaymentSummaryCommandHandler(
    IInvoicesRepository invoiceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertInvoicePaymentSummaryCommand, Result>
{
    public async Task<Result> Handle(
        UpsertInvoicePaymentSummaryCommand cmd,
        CancellationToken cancellationToken)
    {
        var currencyCode = CurrencyCode.Create(cmd.CurrencyCode);
        var paidAmount = Money.Create(cmd.PaidAmount, currencyCode);
        var outstandingAmount = Money.Create(cmd.OutstandingAmount, currencyCode);

        var currentSummary = await invoiceRepository.GetPaymentSummaryAsync(
            cmd.TenantId,
            cmd.InvoiceId,
            cancellationToken);

        if (currentSummary is not null)
        {
            var existingVersionResult = CheckExistingVersion(
                currentSummary,
                cmd,
                paidAmount,
                outstandingAmount);

            if (existingVersionResult is not null)
                return existingVersionResult;
        }

        return await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var summary = await invoiceRepository.GetPaymentSummaryWithLockAsync(
                    cmd.TenantId,
                    cmd.InvoiceId,
                    ct);

                if (summary is null)
                {
                    invoiceRepository.AddPaymentSummary(
                        InvoicePaymentSummary.Create(
                            cmd.TenantId,
                            cmd.InvoiceId,
                            paidAmount,
                            outstandingAmount,
                            cmd.PaymentBalanceVersion,
                            cmd.SourceUpdatedAtUtc,
                            cmd.AppliedAtUtc));

                    return await unitOfWork.SaveChangesAsync(ct);
                }

                var lockedVersionResult = CheckExistingVersion(
                    summary,
                    cmd,
                    paidAmount,
                    outstandingAmount);

                if (lockedVersionResult is not null)
                {
                    return lockedVersionResult;
                }

                summary.ApplySnapshot(
                    paidAmount,
                    outstandingAmount,
                    cmd.PaymentBalanceVersion,
                    cmd.SourceUpdatedAtUtc,
                    cmd.AppliedAtUtc);

                return await unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);
    }

    private static Result? CheckExistingVersion(
        InvoicePaymentSummary summary,
        UpsertInvoicePaymentSummaryCommand cmd,
        Money paidAmount,
        Money outstandingAmount)
    {
        if (summary.PaymentBalanceVersion > cmd.PaymentBalanceVersion)
        {
            return Result.Failure(
                ApplicationErrors.PaymentBalanceVersionNotNewer);
        }

        if (summary.PaymentBalanceVersion != cmd.PaymentBalanceVersion)
            return null;

        var snapshotMatches =
            summary.PaidAmount == paidAmount &&
            summary.OutstandingAmount == outstandingAmount &&
            summary.SourceUpdatedAtUtc == cmd.SourceUpdatedAtUtc.ToUniversalTime();

        return snapshotMatches
            ? Result.Success()
            : Result.Failure(ApplicationErrors.PaymentBalanceVersionConflict);
    }
}
