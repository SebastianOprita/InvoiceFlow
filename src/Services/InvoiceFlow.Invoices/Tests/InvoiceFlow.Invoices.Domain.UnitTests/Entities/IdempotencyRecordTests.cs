using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Entities;

public sealed class IdempotencyRecordTests
{
    private static readonly DateTimeOffset CreatedAt = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_SetsIdentityAndRequestData()
    {
        var tenantId = Guid.CreateVersion7();
        var resourceId = Guid.CreateVersion7();
        var key = IdempotencyKey.Create(" request-1 ");
        var hash = RequestHash.Create(new string('A', 64));

        var record = IdempotencyRecord.Create(tenantId, IdempotencyOperation.CreateInvoice, key, hash, resourceId, CreatedAt);

        record.Id.Should().NotBeEmpty();
        record.TenantId.Should().Be(tenantId);
        record.Operation.Should().Be(IdempotencyOperation.CreateInvoice);
        record.Key.Should().Be(key);
        record.RequestHash.Should().Be(hash);
        record.ResourceId.Should().Be(resourceId);
        record.CreatedAtUtc.Should().Be(CreatedAt);
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        Action act = () => IdempotencyRecord.Create(Guid.Empty, IdempotencyOperation.CreateInvoice,
            IdempotencyKey.Create("key"), RequestHash.Create(new string('A', 64)), Guid.CreateVersion7(), CreatedAt);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsUndefinedOperation()
    {
        Action act = () => IdempotencyRecord.Create(Guid.CreateVersion7(), (IdempotencyOperation)999,
            IdempotencyKey.Create("key"), RequestHash.Create(new string('A', 64)), Guid.CreateVersion7(), CreatedAt);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdempotencyOperationInvalid);
    }

    [Fact]
    public void Create_RejectsEmptyResourceId()
    {
        Action act = () => IdempotencyRecord.Create(Guid.CreateVersion7(), IdempotencyOperation.AddInvoiceLine,
            IdempotencyKey.Create("key"), RequestHash.Create(new string('A', 64)), Guid.Empty, CreatedAt);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.ResourceIdRequired);
    }

    [Fact]
    public void MatchesRequest_ReturnsWhetherRequestHashMatches()
    {
        var hash = RequestHash.Create(new string('A', 64));
        var record = IdempotencyRecord.Create(Guid.CreateVersion7(), IdempotencyOperation.CreateInvoice,
            IdempotencyKey.Create("key"), hash, Guid.CreateVersion7(), CreatedAt);

        record.MatchesRequest(hash).Should().BeTrue();
        record.MatchesRequest(RequestHash.Create(new string('B', 64))).Should().BeFalse();
    }
}
