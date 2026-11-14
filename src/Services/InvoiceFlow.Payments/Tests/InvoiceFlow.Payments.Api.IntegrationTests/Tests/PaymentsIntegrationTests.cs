using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Api.IntegrationTests;

[Collection(PaymentsApiCollection.Name)]
public sealed class PaymentsIntegrationTests : IAsyncLifetime
{
    private readonly PaymentsApiFactory _factory;
    private readonly TestClient _client;
    private readonly Guid _tenantId = Guid.CreateVersion7();
    private string BaseUrl => $"/api/{_tenantId}/payments";
    private string _paymentReadToken = string.Empty;

    public PaymentsIntegrationTests(PaymentsApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _paymentReadToken = TestJwtTokenFactory.CreateAccessToken(_tenantId, SystemPermissions.PaymentRead, _factory.JwtSettings);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public static IEnumerable<object[]> ProtectedEndpoints =>
    [
        [HttpMethod.Get, "/api/{tenantId}/payments"],
        [HttpMethod.Get, "/api/{tenantId}/payments/{paymentId}"]
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

    private static Payment CreatePayment(Guid tenantId) => Payment.Create(
        tenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), CurrencyCode.Create("RON"),
        Money.Create(100m, CurrencyCode.Create("RON")), PaymentMethod.Card,
        PaymentReference.CreateOptional("Integration test reference"), PaymentNotes.CreateOptional("Integration test payment"),
        DateTimeOffset.Parse("2025-01-01T10:00:00Z"));

    private string ResolveUrl(string url) => url
        .Replace("{tenantId}", _tenantId.ToString())
        .Replace("{paymentId}", Guid.CreateVersion7().ToString());
}
