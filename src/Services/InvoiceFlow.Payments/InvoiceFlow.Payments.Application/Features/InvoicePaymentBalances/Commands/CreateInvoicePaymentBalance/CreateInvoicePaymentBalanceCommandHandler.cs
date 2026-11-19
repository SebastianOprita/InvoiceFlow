using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using MediatR;

namespace InvoiceFlow.Payments.Application;

internal sealed class CreateInvoicePaymentBalanceCommandHandler(
    IPaymentsRepository paymentsRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateInvoicePaymentBalanceCommand, Result>
{
    public async Task<Result> Handle(
        CreateInvoicePaymentBalanceCommand cmd,
        CancellationToken cancellationToken)
    {
        var currencyCode = CurrencyCode.Create(cmd.CurrencyCode);
        var totalAmount = Money.Create(cmd.TotalAmount, currencyCode);
        var invoiceNumber = InvoiceNumber.Create(cmd.InvoiceNumber);

        var existingBalance = await paymentsRepository.GetInvoicePaymentBalanceAsync(
            cmd.TenantId,
            cmd.InvoiceId,
            cancellationToken);

        if (existingBalance is not null)
        {
            var valuesMatch =
                existingBalance.CustomerId == cmd.CustomerId &&
                existingBalance.InvoiceNumber == invoiceNumber &&
                existingBalance.CurrencyCode == currencyCode &&
                existingBalance.TotalAmount == totalAmount &&
                existingBalance.CreatedAtUtc == cmd.CreatedAtUtc.ToUniversalTime();

            return valuesMatch
                ? Result.Success()
                : Result.Failure(ApplicationErrors.InvoicePaymentBalanceConflict);
        }

        paymentsRepository.AddInvoicePaymentBalance(
            InvoicePaymentBalance.Create(
                cmd.TenantId,
                cmd.InvoiceId,
                cmd.CustomerId,
                invoiceNumber,
                currencyCode,
                totalAmount,
                cmd.CreatedAtUtc));

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
