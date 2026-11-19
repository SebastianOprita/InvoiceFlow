using FluentAssertions;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class GetPaymentsQueryHandlerTests
{
    [Fact]
    public async Task Returns_mapped_payments_for_tenant()
    {
        var payment = PaymentMapperTests.CreatePayment();
        var repo = new Mock<IPaymentsRepository>();
        repo.Setup(x => x.GetAllPaymentsWithAllocationsAsync(payment.TenantId, It.IsAny<CancellationToken>())).ReturnsAsync([payment]);
        var result = await new GetPaymentsQueryHandler(repo.Object).Handle(new(payment.TenantId), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task Returns_empty_list_when_no_payments()
    {
        var repo = new Mock<IPaymentsRepository>();
        repo.Setup(x => x.GetAllPaymentsWithAllocationsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var result = await new GetPaymentsQueryHandler(repo.Object).Handle(new(Guid.CreateVersion7()), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
