using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application;

internal class GetPaymentsQueryHandler(IPaymentsRepository paymentsRepository)
    : IRequestHandler<GetPaymentsQuery, Result<List<PaymentDto>>>
{
    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsQuery qry, CancellationToken cancellationToken)
    {
        var payments = await paymentsRepository.GetAllPaymentsWithAllocationsAsync(qry.TenantId, cancellationToken);
        var paymentDtos = payments.Select(x => x.ToDto()).ToList();

        return Result<List<PaymentDto>>.Success(paymentDtos);
    }
}
