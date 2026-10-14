using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public sealed record CreateCustomerCommand(
    Guid TenantId,
    string CustomerCode,
    string Name,
    CustomerContact CustomerContact,
    CustomerTaxDetails CustomerTaxDetails,
    CustomerAddress CustomerAddress,
    string CurrencyCode,
    CustomerCreditPolicy CustomerCreditPolicy) : IRequest<Result<CustomerDto>>;
