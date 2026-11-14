using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application.Features.Payments.Queries.GetPaymentById;

internal class GetPaymentByIdQueryHandler(IPaymentsRepository paymentsRepository)
    : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery qry, CancellationToken cancellationToken)
    {
        var payment = await paymentsRepository.GetPaymentByIdAsync(qry.TenantId, qry.PaymentId, cancellationToken);

        if (payment is null)
            return Result<PaymentDto>.Failure(ApplicationErrors.PaymentNotFound);

        return Result<PaymentDto>.Success(payment.ToDto());
    }
}
