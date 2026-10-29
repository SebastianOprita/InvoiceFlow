using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IRequestHasher
{
    Result<RequestHash> Hash<TRequest>(TRequest request);
}
