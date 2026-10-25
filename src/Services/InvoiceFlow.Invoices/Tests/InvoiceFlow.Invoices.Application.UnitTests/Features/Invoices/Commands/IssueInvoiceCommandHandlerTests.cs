using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class IssueInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepository = new();
    private readonly Mock<IInvoiceNumberRepository> _invoiceNumberRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISystemDateTimeProvider> _dateTimeProvider = new();
    private readonly IssueInvoiceCommandHandler _handler;

    public IssueInvoiceCommandHandlerTests()
    {
        _handler = new(_invoiceRepository.Object, _invoiceNumberRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object);
        _unitOfWork.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task<Result>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<Result>> operation, CancellationToken ct) => operation(ct));
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        _dateTimeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2025, 3, 4, 23, 30, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Handle_WithDraftInvoice_ShouldAllocateNumberIssueAndSave()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var command = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoiceRepository.Setup(x => x.GetForIssuingAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _invoiceNumberRepository.Setup(x => x.AllocateNextAsync(command.TenantId, 2025, It.IsAny<CancellationToken>())).ReturnsAsync("INV-2025-0001");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.Number!.Value.Should().Be("INV-2025-0001");
        invoice.IssueDate.Should().Be(new DateOnly(2025, 3, 4));
        _invoiceNumberRepository.Verify(x => x.AllocateNextAsync(command.TenantId, 2025, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvoiceDoesNotExist_ShouldReturnNotFound()
    {
        var command = new IssueInvoiceCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        _invoiceRepository.Setup(x => x.GetForIssuingAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ApplicationErrors.InvoiceNotFound.Code);
        _invoiceNumberRepository.Verify(x => x.AllocateNextAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvoiceIsAlreadyIssued_ShouldSucceedWithoutAllocatingNumber()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        invoice.Issue(InvoiceNumber.Create("INV-2025-0001"), new DateOnly(2025, 1, 1), new DateTimeOffset(2025, 1, 1, 11, 0, 0, TimeSpan.Zero));
        var command = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoiceRepository.Setup(x => x.GetForIssuingAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _invoiceNumberRepository.Verify(x => x.AllocateNextAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvoiceIsCancelled_ShouldReturnConflict()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        invoice.Issue(InvoiceNumber.Create("INV-2025-0001"), new DateOnly(2025, 1, 1), new DateTimeOffset(2025, 1, 1, 11, 0, 0, TimeSpan.Zero));
        invoice.Cancel(new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero));
        var command = new IssueInvoiceCommand(invoice.TenantId, invoice.Id);
        _invoiceRepository.Setup(x => x.GetForIssuingAsync(command.TenantId, command.InvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ApplicationErrors.InvoiceCannotBeIssued.Code);
        _invoiceNumberRepository.Verify(x => x.AllocateNextAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
