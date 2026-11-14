using FluentAssertions;
using FluentValidation.TestHelper;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Application.Features.Payments.Commands.CreatePayment;
using InvoiceFlow.Payments.Application.Features.Payments.Queries.GetPaymentById;
using InvoiceFlow.Payments.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _sut = new();
    private static readonly CreatePaymentCommand Valid = new("key", Guid.CreateVersion7(), "CUST-001", "RON", 12.34m, PaymentMethod.Card, "ref", "notes");

    [Fact] public void Valid_command_has_no_errors() => _sut.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();
    [Theory, InlineData(""), InlineData(null)] public void Requires_idempotency_key(string? value) => _sut.TestValidate(Valid with { IdempotencyKey = value! }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    [Fact] public void Limits_idempotency_key() => _sut.TestValidate(Valid with { IdempotencyKey = new string('k', 129) }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    [Fact] public void Requires_tenant() => _sut.TestValidate(Valid with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);
    [Theory, InlineData(""), InlineData(null)] public void Requires_customer_code(string? value) => _sut.TestValidate(Valid with { CustomerCode = value! }).ShouldHaveValidationErrorFor(x => x.CustomerCode);
    [Fact] public void Limits_customer_code() => _sut.TestValidate(Valid with { CustomerCode = new string('c', 37) }).ShouldHaveValidationErrorFor(x => x.CustomerCode);
    [Theory, InlineData(""), InlineData("ZZZ")] public void Requires_supported_currency(string value) => _sut.TestValidate(Valid with { CurrencyCode = value }).ShouldHaveValidationErrorFor(x => x.CurrencyCode);
    [Theory, InlineData(0), InlineData(-1)] public void Requires_positive_amount(decimal value) => _sut.TestValidate(Valid with { Amount = value }).ShouldHaveValidationErrorFor(x => x.Amount);
    [Fact] public void Rejects_amount_with_excess_currency_precision() => _sut.TestValidate(Valid with { Amount = 1.234m }).ShouldHaveValidationErrorFor(x => x.Amount);
    [Fact] public void Rejects_invalid_payment_method() => _sut.TestValidate(Valid with { PaymentMethod = (PaymentMethod)99 }).ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    [Fact] public void Limits_reference() => _sut.TestValidate(Valid with { Reference = new string('r', PaymentReference.MaxLength + 1) }).ShouldHaveValidationErrorFor(x => x.Reference);
    [Fact] public void Limits_notes() => _sut.TestValidate(Valid with { Notes = new string('n', PaymentNotes.MaxLength + 1) }).ShouldHaveValidationErrorFor(x => x.Notes);
}

public sealed class PaymentQueryValidatorTests
{
    [Fact]
    public void Get_payment_by_id_requires_tenant_and_payment()
    {
        var result = new GetPaymentByIdQueryValidator().TestValidate(new GetPaymentByIdQuery(Guid.Empty, Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }
    [Fact] public void Get_payments_requires_tenant() => new GetPaymentsQueryValidator().TestValidate(new GetPaymentsQuery(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.TenantId);
    [Fact] public void Get_payments_accepts_tenant() => new GetPaymentsQueryValidator().TestValidate(new GetPaymentsQuery(Guid.CreateVersion7())).ShouldNotHaveAnyValidationErrors();
}

public sealed class PaymentMapperTests
{
    [Fact]
    public void Maps_payment_fields_and_empty_allocations()
    {
        var payment = CreatePayment();
        var dto = payment.ToDto();
        dto.TenantId.Should().Be(payment.TenantId);
        dto.Id.Should().Be(payment.Id);
        dto.CustomerId.Should().Be(payment.CustomerId);
        dto.CurrencyCode.Should().Be("RON");
        dto.Amount.Should().Be(25m);
        dto.AllocatedAmount.Should().Be(0m);
        dto.UnallocatedAmount.Should().Be(25m);
        dto.Method.Should().Be(PaymentMethod.Card);
        dto.Reference.Should().Be("INVEST-1");
        dto.Notes.Should().Be("test note");
        dto.Allocations.Should().BeEmpty();
    }

    internal static Payment CreatePayment() => Payment.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), CurrencyCode.Create("RON"), Money.Create(25m, CurrencyCode.Create("RON")), PaymentMethod.Card, PaymentReference.Create("INVEST-1"), PaymentNotes.Create("test note"), DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
}

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
