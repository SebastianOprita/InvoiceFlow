using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class CreateInvoicePaymentBalanceCommandHandlerTests
{
    private static readonly DateTimeOffset CreatedAtUtc =
        DateTimeOffset.Parse("2025-01-01T10:00:00Z");

    [Fact]
    public async Task Creates_and_saves_balance_when_missing()
    {
        var command = CreateCommand();
        InvoicePaymentBalance? addedBalance = null;
        var repository = new Mock<IPaymentsRepository>();
        repository.Setup(x => x.GetInvoicePaymentBalanceAsync(
                command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoicePaymentBalance?)null);
        repository.Setup(x => x.AddInvoicePaymentBalance(It.IsAny<InvoicePaymentBalance>()))
            .Callback<InvoicePaymentBalance>(balance => addedBalance = balance);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var sut = new CreateInvoicePaymentBalanceCommandHandler(repository.Object, unitOfWork.Object);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        addedBalance.Should().NotBeNull();
        addedBalance!.TenantId.Should().Be(command.TenantId);
        addedBalance.InvoiceId.Should().Be(command.InvoiceId);
        addedBalance.CustomerId.Should().Be(command.CustomerId);
        addedBalance.InvoiceNumber.Value.Should().Be(command.InvoiceNumber);
        addedBalance.CurrencyCode.Value.Should().Be(command.CurrencyCode);
        addedBalance.TotalAmount.Amount.Should().Be(command.TotalAmount);
        addedBalance.CreatedAtUtc.Should().Be(command.CreatedAtUtc);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task<Result>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<System.Data.IsolationLevel>()), Times.Never);
    }

    [Fact]
    public async Task Returns_success_without_saving_when_existing_values_match()
    {
        var command = CreateCommand();
        var existingBalance = CreateBalance(command);
        var repository = new Mock<IPaymentsRepository>();
        repository.Setup(x => x.GetInvoicePaymentBalanceAsync(
                command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBalance);
        var unitOfWork = new Mock<IUnitOfWork>();
        var sut = new CreateInvoicePaymentBalanceCommandHandler(repository.Object, unitOfWork.Object);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.Verify(x => x.AddInvoicePaymentBalance(It.IsAny<InvoicePaymentBalance>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_conflict_without_saving_when_existing_values_differ()
    {
        var command = CreateCommand();
        var existingBalance = CreateBalance(command with { TotalAmount = command.TotalAmount + 1m });
        var repository = new Mock<IPaymentsRepository>();
        repository.Setup(x => x.GetInvoicePaymentBalanceAsync(
                command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBalance);
        var unitOfWork = new Mock<IUnitOfWork>();
        var sut = new CreateInvoicePaymentBalanceCommandHandler(repository.Object, unitOfWork.Object);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoicePaymentBalanceConflict);
        repository.Verify(x => x.AddInvoicePaymentBalance(It.IsAny<InvoicePaymentBalance>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreateInvoicePaymentBalanceCommand CreateCommand() => new(
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        "INV-001",
        "RON",
        125.50m,
        CreatedAtUtc);

    private static InvoicePaymentBalance CreateBalance(CreateInvoicePaymentBalanceCommand command)
    {
        var currency = CurrencyCode.Create(command.CurrencyCode);
        return InvoicePaymentBalance.Create(
            command.TenantId,
            command.InvoiceId,
            command.CustomerId,
            InvoiceNumber.Create(command.InvoiceNumber),
            currency,
            Money.Create(command.TotalAmount, currency),
            command.CreatedAtUtc);
    }
}
