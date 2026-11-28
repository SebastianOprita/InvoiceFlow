using System.Net;
using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Api.IntegrationTests;

[Collection(PaymentsApiCollection.Name)]
public sealed class PaymentsApiTests : IAsyncLifetime
{
    private readonly PaymentsApiFactory _factory;
    private readonly TestClient _client;
    private readonly Guid _tenantId = Guid.CreateVersion7();
    private string BaseUrl => $"/api/{_tenantId}/payments";
    private string _paymentReadToken = string.Empty;
    private string _paymentAccessToken = string.Empty;

    public PaymentsApiTests(PaymentsApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _paymentAccessToken = TestJwtTokenFactory.CreateAccessToken(
            _tenantId,
            SystemPermissions.PaymentRead | SystemPermissions.PaymentCreate | SystemPermissions.PaymentAddAllocation | SystemPermissions.PaymentPost,
            _factory.JwtSettings);
        _paymentReadToken = TestJwtTokenFactory.CreateAccessToken(_tenantId, SystemPermissions.PaymentRead, _factory.JwtSettings);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public static IEnumerable<object[]> ProtectedEndpoints =>
    [
        [HttpMethod.Get, "/api/{tenantId}/payments"],
        [HttpMethod.Get, "/api/{tenantId}/payments/{paymentId}"],
        [HttpMethod.Post, "/api/{tenantId}/payments"],
        [HttpMethod.Post, "/api/{tenantId}/payments/{paymentId}/Post"],
        [HttpMethod.Post, "/api/{tenantId}/payments/{paymentId}/allocations"]
    ];

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_return_unauthorized_without_token(HttpMethod method, string url)
    {
        var response = await _client.SendAsync(method, ResolveUrl(url), "", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_return_forbidden_without_permission(HttpMethod method, string url)
    {
        var token = TestJwtTokenFactory.CreateAccessToken(_tenantId, SystemPermissions.None, _factory.JwtSettings);
        var response = await _client.SendAsync(method, ResolveUrl(url), token, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_returns_only_payments_for_the_route_tenant()
    {
        var payment = CreatePayment(_tenantId);
        await _factory.SeedPaymentAsync(payment);
        await _factory.SeedPaymentAsync(CreatePayment(Guid.CreateVersion7()));

        var response = await _client.SendAsync(HttpMethod.Get, BaseUrl, _paymentReadToken, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payments = await response.Content.ReadFromJsonAsync<List<PaymentDto>>(TestContext.Current.CancellationToken);
        payments.Should().ContainSingle();
        payments![0].Id.Should().Be(payment.Id);
        payments[0].TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task GetById_returns_payment_when_it_exists()
    {
        var payment = CreatePayment(_tenantId);
        await _factory.SeedPaymentAsync(payment);

        var response = await _client.SendAsync(HttpMethod.Get, $"{BaseUrl}/{payment.Id}", _paymentReadToken, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentDto>(TestContext.Current.CancellationToken);
        body.Should().NotBeNull();
        body!.Id.Should().Be(payment.Id);
        body.TenantId.Should().Be(_tenantId);
        body.Amount.Should().Be(100m);
    }

    [Fact]
    public async Task GetById_returns_not_found_when_payment_does_not_exist()
    {
        var response = await _client.SendAsync(HttpMethod.Get, $"{BaseUrl}/{Guid.CreateVersion7()}", _paymentReadToken, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_with_active_customer_returns_created_payment()
    {
        var response = await _client.SendAsync(
            HttpMethod.Post,
            BaseUrl,
            _paymentAccessToken,
            new CreatePaymentRequest("CUST001", "RON", 100m, PaymentMethod.Card, "Integration reference", "Integration payment"),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payment = await response.Content.ReadFromJsonAsync<PaymentDto>(TestContext.Current.CancellationToken);
        payment.Should().NotBeNull();
        payment!.TenantId.Should().Be(_tenantId);
        payment.Amount.Should().Be(100m);
        payment.Status.Should().Be(PaymentLifecycleStatus.Draft);
        payment.Number.Should().BeNull();
    }

    [Fact]
    public async Task Post_when_payment_is_draft_returns_no_content_and_updates_payment()
    {
        var createResponse = await _client.SendAsync(
            HttpMethod.Post,
            BaseUrl,
            _paymentAccessToken,
            new CreatePaymentRequest("CUST001", "RON", 100m, PaymentMethod.Card, "Integration reference", "Integration payment"),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>(TestContext.Current.CancellationToken);

        var postResponse = await _client.SendAsync(
            HttpMethod.Post,
            $"{BaseUrl}/{createdPayment!.Id}/Post",
            _paymentAccessToken,
            TestContext.Current.CancellationToken);

        postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await _client.SendAsync(
            HttpMethod.Get,
            $"{BaseUrl}/{createdPayment.Id}",
            _paymentReadToken,
            TestContext.Current.CancellationToken);
        var postedPayment = await getResponse.Content.ReadFromJsonAsync<PaymentDto>(TestContext.Current.CancellationToken);
        postedPayment.Should().NotBeNull();
        postedPayment!.Status.Should().Be(PaymentLifecycleStatus.Posted);
        postedPayment.Number.Should().NotBeNullOrEmpty();
        postedPayment.PaymentDate.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAllocation_returns_created_payment_with_specified_partial_allocation()
    {
        var payment = CreatePayment(_tenantId);
        payment.Post(
            PaymentNumber.Create("PAY-2025-000001"),
            new DateOnly(2025, 1, 2),
            Guid.CreateVersion7(),
            DateTimeOffset.Parse("2025-01-02T10:00:00Z"));
        var balance = InvoicePaymentBalance.Create(
            _tenantId,
            Guid.CreateVersion7(),
            payment.CustomerId,
            InvoiceNumber.Create("INV-2025-000001"),
            payment.CurrencyCode,
            Money.Create(75m, payment.CurrencyCode),
            DateTimeOffset.Parse("2025-01-01T10:00:00Z"));
        await _factory.SeedPaymentAsync(payment);
        await _factory.SeedInvoicePaymentBalanceAsync(balance);

        var response = await _client.SendAsync(
            HttpMethod.Post,
            $"{BaseUrl}/{payment.Id}/allocations",
            _paymentAccessToken,
            new AddPaymentAllocationRequest(balance.InvoiceId, 40m),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updatedPayment = await response.Content.ReadFromJsonAsync<PaymentDto>(TestContext.Current.CancellationToken);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Id.Should().Be(payment.Id);
        updatedPayment.AllocatedAmount.Should().Be(40m);
        updatedPayment.UnallocatedAmount.Should().Be(60m);
        updatedPayment.Allocations.Should().ContainSingle(allocation =>
            allocation.InvoiceId == balance.InvoiceId && allocation.Amount == 40m);
    }

    private static Payment CreatePayment(Guid tenantId) => Payment.Create(
        tenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), CurrencyCode.Create("RON"),
        Money.Create(100m, CurrencyCode.Create("RON")), PaymentMethod.Card,
        PaymentReference.CreateOptional("Integration test reference"), PaymentNotes.CreateOptional("Integration test payment"),
        Guid.CreateVersion7(),
        DateTimeOffset.Parse("2025-01-01T10:00:00Z"));

    private string ResolveUrl(string url) => url
        .Replace("{tenantId}", _tenantId.ToString())
        .Replace("{paymentId}", Guid.CreateVersion7().ToString());
}
