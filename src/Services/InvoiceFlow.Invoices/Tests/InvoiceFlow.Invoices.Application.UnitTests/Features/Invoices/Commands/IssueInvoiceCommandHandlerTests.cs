using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class IssueInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoices = new();
    private readonly Mock<IInvoiceNumberRepository> _numbers = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISystemDateTimeProvider> _clock = new();
    private readonly IssueInvoiceCommandHandler _sut;

    public IssueInvoiceCommandHandlerTests()
    {
        _clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2025, 1, 2, 10, 0, 0, TimeSpan.Zero));
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<Result>>>(), It.IsAny<CancellationToken>(), It.IsAny<System.Data.IsolationLevel>()))
            .Returns((Func<CancellationToken, Task<Result>> action, CancellationToken ct, System.Data.IsolationLevel _) => action(ct));
        _sut = new(_invoices.Object, _numbers.Object, _unitOfWork.Object, _clock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenInvoiceDoesNotExist()
    {
        var cmd = new IssueInvoiceCommand(InvoiceTestData.TenantId, Guid.CreateVersion7());
        _invoices.Setup(x => x.GetInvoiceWithLockAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoiceNotFound);
        _numbers.Verify(x => x.AllocateNextWithLockAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessWithoutSaving_WhenAlreadyIssued()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        invoice.Issue(InvoiceNumber.Create("2025-0001"), new DateOnly(2025, 1, 2), _clock.Object.GetUtcNow());
        var cmd = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoices.Setup(x => x.GetInvoiceWithLockAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _numbers.Verify(x => x.AllocateNextWithLockAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsCannotBeIssued_WhenInvoiceIsCancelled()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        invoice.Issue(InvoiceNumber.Create("2025-0001"), new DateOnly(2025, 1, 2), _clock.Object.GetUtcNow());
        invoice.Cancel(_clock.Object.GetUtcNow().AddDays(1));
        var cmd = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoices.Setup(x => x.GetInvoiceWithLockAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoiceCannotBeIssued);
        _numbers.Verify(x => x.AllocateNextWithLockAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IssuesDraftAndSaves()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var cmd = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoices.Setup(x => x.GetInvoiceWithLockAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _numbers.Setup(x => x.AllocateNextWithLockAsync(cmd.TenantId, 2025, It.IsAny<CancellationToken>())).ReturnsAsync("2025-0001");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Issued);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessAfterConcurrencyConflict_WhenAnotherRequestIssuedInvoice()
    {
        var draft = InvoiceTestData.DraftInvoice();
        var issued = InvoiceTestData.DraftInvoice();
        issued.Issue(InvoiceNumber.Create("2025-0001"), new DateOnly(2025, 1, 2), _clock.Object.GetUtcNow());
        var cmd = new IssueInvoiceCommand(draft.TenantId, draft.Id);
        _invoices.SetupSequence(x => x.GetInvoiceWithLockAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(draft);
        _invoices.Setup(x => x.GetInvoiceByIdAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(issued);
        _numbers.Setup(x => x.AllocateNextWithLockAsync(cmd.TenantId, 2025, It.IsAny<CancellationToken>())).ReturnsAsync("2025-0002");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure(ApplicationErrors.ConcurrencyConflict));

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.ClearTrackedChanges(), Times.Once);
    }

    [Fact]
    public async Task Handle_PreservesConcurrencyConflict_WhenReloadedInvoiceIsNotIssued()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var cmd = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoices.Setup(x => x.GetInvoiceWithLockAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _invoices.Setup(x => x.GetInvoiceByIdAsync(cmd.TenantId, cmd.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(InvoiceTestData.DraftInvoice());
        _numbers.Setup(x => x.AllocateNextWithLockAsync(cmd.TenantId, 2025, It.IsAny<CancellationToken>())).ReturnsAsync("2025-0001");
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure(ApplicationErrors.ConcurrencyConflict));

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.ConcurrencyConflict);
    }
}
