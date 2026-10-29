using FluentAssertions;
using InvoiceFlow.Invoices.Api;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class InvoiceMapperTests
{
    [Fact]
    public void ToCommand_MapsCreateInvoiceRequestAndContext()
    {
        var request = new CreateInvoiceRequest("CUST-001", "Quarterly services");
        var idempotencyKey = "request-key";
        var tenantId = Guid.CreateVersion7();

        var command = request.ToCommand(idempotencyKey, tenantId);

        command.IdempotencyKey.Should().Be(idempotencyKey);
        command.TenantId.Should().Be(tenantId);
        command.CustomerCode.Should().Be(request.CustomerCode);
        command.Notes.Should().Be(request.InvoiceNotes);
    }

    [Fact]
    public void ToCommand_MapsAddInvoiceLineRequestAndContext()
    {
        var request = new AddInvoiceLineRequest("PROD-001", 2.5m, 10m);
        var idempotencyKey = "line-request-key";
        var tenantId = Guid.CreateVersion7();
        var invoiceId = Guid.CreateVersion7();

        var command = request.ToCommand(idempotencyKey, tenantId, invoiceId);

        command.IdempotencyKey.Should().Be(idempotencyKey);
        command.TenantId.Should().Be(tenantId);
        command.InvoiceId.Should().Be(invoiceId);
        command.ProductCode.Should().Be(request.ProductCode);
        command.Quantity.Should().Be(request.Quantity);
        command.DiscountRate.Should().Be(request.DiscountRate);
    }

    [Fact]
    public void ToCommand_PreservesNullOptionalFields()
    {
        var createCommand = new CreateInvoiceRequest("CUST-001", null!).ToCommand("key", Guid.CreateVersion7());
        var lineCommand = new AddInvoiceLineRequest("PROD-001", 1m, null).ToCommand("key", Guid.CreateVersion7(), Guid.CreateVersion7());

        createCommand.Notes.Should().BeNull();
        lineCommand.DiscountRate.Should().BeNull();
    }
}
