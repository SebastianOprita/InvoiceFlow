using FluentAssertions;
using InvoiceFlow.Payments.Application.Features.Payments.Queries.GetPaymentById;
using InvoiceFlow.Payments.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class GetPaymentByIdQueryHandlerTests
{
    [Fact]
    public async Task Returns_payment_when_found()
    {
        var payment = PaymentMapperTests.CreatePayment();
        var repo = new Mock<IPaymentsRepository>();
        repo.Setup(x => x.GetPaymentByIdAsync(payment.TenantId, payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        var result = await new GetPaymentByIdQueryHandler(repo.Object).Handle(new(payment.TenantId, payment.Id), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task Returns_not_found_when_missing()
    {
        var repo = new Mock<IPaymentsRepository>();
        repo.Setup(x => x.GetPaymentByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);
        var result = await new GetPaymentByIdQueryHandler(repo.Object).Handle(new(Guid.CreateVersion7(), Guid.CreateVersion7()), CancellationToken.None);
        result.Error.Should().Be(ApplicationErrors.PaymentNotFound);
    }
}
