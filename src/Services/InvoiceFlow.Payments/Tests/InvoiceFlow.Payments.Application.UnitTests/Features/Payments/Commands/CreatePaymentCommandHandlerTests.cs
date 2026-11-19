using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class CreatePaymentCommandHandlerTests
{
    [Fact]
    public async Task Returns_customer_error_without_starting_transaction()
    {
        var tenantId = Guid.CreateVersion7();
        var command = new CreatePaymentCommand("key", tenantId, "CUST-001", "RON", 25m, PaymentMethod.Card, "ref", "notes");
        var idempotency = new Mock<IIdempotencyRecordsRepository>();
        idempotency.Setup(x => x.GetIdempotencyRecordAsync(tenantId, IdempotencyOperation.CreatePayment, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdempotencyRecord?)null);
        var customers = new Mock<ICustomersService>();
        customers.Setup(x => x.GetCustomerAsync(tenantId, command.CustomerCode, It.IsAny<CancellationToken>())).ReturnsAsync(Result<PaymentCustomerSnapshot>.Failure(ApplicationErrors.CustomerNotFound));
        var uow = new Mock<IUnitOfWork>();
        var hasher = new FixedHasher(RequestHash.Create(new string('A', 64)));
        var sut = new CreatePaymentCommandHandler(new Mock<IPaymentsRepository>().Object, idempotency.Object, customers.Object, hasher, uow.Object, new Mock<ISystemDateTimeProvider>().Object);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.CustomerNotFound);
        uow.Verify(x => x.ExecuteInTransactionAsync<PaymentDto>(It.IsAny<Func<CancellationToken, Task<Result<PaymentDto>>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Replays_matching_idempotent_payment_without_fetching_customer()
    {
        var payment = PaymentMapperTests.CreatePayment();
        var repository = new Mock<IPaymentsRepository>();
        var idempotency = new Mock<IIdempotencyRecordsRepository>();
        var customers = new Mock<ICustomersService>();
        var command = new CreatePaymentCommand("key", payment.TenantId, "CUST-001", "RON", 25m, PaymentMethod.Card, "INVEST-1", "test note");
        var hash = RequestHash.Create(new string('A', 64));
        var record = IdempotencyRecord.Create(command.TenantId, IdempotencyOperation.CreatePayment, IdempotencyKey.Create(command.IdempotencyKey), hash, payment.Id, payment.CreatedAtUtc);
        idempotency.Setup(x => x.GetIdempotencyRecordAsync(command.TenantId, IdempotencyOperation.CreatePayment, It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(record);
        repository.Setup(x => x.GetPaymentByIdAsync(command.TenantId, payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        var uow = new Mock<IUnitOfWork>();
        var clock = new Mock<ISystemDateTimeProvider>();
        var sut = new CreatePaymentCommandHandler(repository.Object, idempotency.Object, customers.Object, new FixedHasher(hash), uow.Object, clock.Object);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(payment.Id);
        customers.VerifyNoOtherCalls();
        repository.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Never);
        uow.Verify(x => x.ExecuteInTransactionAsync<PaymentDto>(It.IsAny<Func<CancellationToken, Task<Result<PaymentDto>>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    private sealed class FixedHasher(RequestHash hash) : IRequestHasher
    {
        public Result<RequestHash> Hash<TRequest>(TRequest request) => Result<RequestHash>.Success(hash);
    }
}
