using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Customers.Application;

public record UpdateCustomerCommand(
    Guid TenantId,
    Guid CustomerId,
    string? Name,
    CustomerContact? CustomerContact,
    CustomerAddress? CustomerAddress,
    CustomerCreditPolicy? CustomerCreditPolicy) : IRequest<Result<CustomerDto>>;
