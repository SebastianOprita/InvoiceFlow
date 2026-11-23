using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using MediatR;

namespace InvoiceFlow.Payments.Application;

internal sealed class PostPaymentCommandHandler(
    IPaymentsRepository paymentsRepository,
    IPaymentNumbersRepository paymentNumbersRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<PostPaymentCommand, Result>
{
    public async Task<Result> Handle(PostPaymentCommand cmd, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var payment = await paymentsRepository.GetPaymentWithLockAsync(
                    cmd.TenantId,
                    cmd.PaymentId,
                    ct);

                if (payment is null)
                    return Result.Failure(ApplicationErrors.PaymentNotFound);

                if (payment.Status == PaymentLifecycleStatus.Posted)
                    return Result.Success();

                if (payment.Status != PaymentLifecycleStatus.Draft)
                    return Result.Failure(ApplicationErrors.PaymentCannotBePosted);

                var now = timeProvider.GetUtcNow();
                var paymentDate = DateOnly.FromDateTime(now.UtcDateTime);

                var paymentNumber = await paymentNumbersRepository.AllocateNextWithLockAsync(
                    cmd.TenantId,
                    FiscalYear.Create(now.UtcDateTime.Year),
                    ct);

                payment.Post(PaymentNumber.Create(paymentNumber), paymentDate, cmd.PostedBy, now);

                return await unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);

        if (result.IsFailure && result.Error == ApplicationErrors.ConcurrencyConflict)
        {
            unitOfWork.ClearTrackedChanges();

            var currentPayment = await paymentsRepository.GetPaymentByIdAsync(
                cmd.TenantId,
                cmd.PaymentId,
                cancellationToken);

            if (currentPayment?.Status == PaymentLifecycleStatus.Posted)
                return Result.Success();
        }

        return result;
    }
}
