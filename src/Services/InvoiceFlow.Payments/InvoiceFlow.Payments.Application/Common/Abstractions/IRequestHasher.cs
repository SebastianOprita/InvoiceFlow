using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface IRequestHasher
{
    Result<RequestHash> Hash<TRequest>(TRequest request);
}
