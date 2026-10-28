using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Invoices.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Api.IntegrationTests;

[Collection(InvoicesApiCollection.Name)]
public sealed class InvoicesApiTests : IAsyncLifetime
{
    private readonly InvoicesApiFactory _factory;
    private readonly TestClient _client;
    private readonly Guid _tenantId = Guid.CreateVersion7();
    private string BaseUrl => $"/api/{_tenantId}/invoices";
    private string _invoiceAccessToken = string.Empty;

    public InvoicesApiTests(InvoicesApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _invoiceAccessToken = TestJwtTokenFactory.CreateAccessToken(
            _tenantId,
            SystemPermissions.InvoiceRead |
            SystemPermissions.InvoiceCreate |
            SystemPermissions.InvoiceIssue,
            _factory.JwtSettings);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public static IEnumerable<object[]> ProtectedEndpoints =>
    [
        [HttpMethod.Get, "/api/{tenantId}/invoices"],
        [HttpMethod.Get, "/api/{tenantId}/invoices/{invoiceId}"],
        [HttpMethod.Post, "/api/{tenantId}/invoices"],
        [HttpMethod.Post, "/api/{tenantId}/invoices/{invoiceId}/line"],
        [HttpMethod.Post, "/api/{tenantId}/invoices/{invoiceId}/Issue"]
    ];

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_Should_ReturnUnauthorized_WhenTokenIsEmpty(HttpMethod method, string url)
    {
        url = ResolveUrl(url);

        var response = await _client.SendAsync(
            method, url, "", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_Should_ReturnForbidden_WhenTokenHasNoPermissions(HttpMethod method, string url)
    {
        url = ResolveUrl(url);
        var token = TestJwtTokenFactory.CreateAccessToken(
            _tenantId, SystemPermissions.None, _factory.JwtSettings);

        var response = await _client.SendAsync(
            method, url, token, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Issue_Should_ReturnForbidden_WhenTokenHasOnlyCustomerStatusPermission()
    {
        var token = TestJwtTokenFactory.CreateAccessToken(
            _tenantId,
            SystemPermissions.CustomerStatusManage,
            _factory.JwtSettings);

        var response = await _client.SendAsync(
            HttpMethod.Post,
            $"{BaseUrl}/{Guid.CreateVersion7()}/Issue",
            token,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_Should_ReturnOk()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get, BaseUrl, _invoiceAccessToken, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenInvoiceExists_Should_ReturnOk()
    {
        var created = await CreateInvoiceAsync();

        var response = await _client.SendAsync(
            HttpMethod.Get,
            $"{BaseUrl}/{created.Id}",
            _invoiceAccessToken,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(TestContext.Current.CancellationToken);
        invoice.Should().NotBeNull();
        invoice!.Id.Should().Be(created.Id);
        invoice.TenantId.Should().Be(_tenantId);
        invoice.CustomerCode.Should().Be("CUST001");
    }

    [Fact]
    public async Task GetById_WhenInvoiceDoesNotExist_Should_ReturnNotFound()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            $"{BaseUrl}/{Guid.CreateVersion7()}",
            _invoiceAccessToken,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithActiveCustomer_Should_ReturnCreated()
    {
        var response = await _client.SendAsync(
            HttpMethod.Post,
            BaseUrl,
            _invoiceAccessToken,
            new CreateInvoiceRequest("CUST001", "Integration test invoice"),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(TestContext.Current.CancellationToken);
        invoice.Should().NotBeNull();
        invoice!.TenantId.Should().Be(_tenantId);
        invoice.CustomerCode.Should().Be("CUST001");
        invoice.Status.Should().Be(InvoiceStatus.Draft);
    }

    [Fact]
    public async Task Create_WithInactiveCustomer_Should_ReturnConflict()
    {
        var response = await _client.SendAsync(
            HttpMethod.Post,
            BaseUrl,
            _invoiceAccessToken,
            new CreateInvoiceRequest("CUST002", "Integration test invoice"),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .Should().Contain(ApplicationErrors.CustomerInactive.Code);
    }

    [Fact]
    public async Task AddLine_WithActiveProduct_Should_ReturnCreated()
    {
        var invoice = await CreateInvoiceAsync();

        var response = await AddLineAsync(invoice.Id, "product001");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updatedInvoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(TestContext.Current.CancellationToken);
        updatedInvoice.Should().NotBeNull();
        updatedInvoice!.Lines.Should().ContainSingle();
        updatedInvoice.Lines.Single().ProductCode.Should().Be("product001");
    }

    [Fact]
    public async Task AddLine_WithInactiveProduct_Should_ReturnConflict()
    {
        var invoice = await CreateInvoiceAsync();

        var response = await AddLineAsync(invoice.Id, "product002");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .Should().Contain(ApplicationErrors.ProductInactive.Code);
    }

    [Fact]
    public async Task Issue_WhenInvoiceHasLine_ShouldReturnNoContentAndUpdateStatus()
    {
        var invoice = await CreateInvoiceAsync();
        var addLineResponse = await AddLineAsync(invoice.Id, "product001");
        addLineResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var issueResponse = await _client.SendAsync(
            HttpMethod.Post,
            $"{BaseUrl}/{invoice.Id}/Issue",
            _invoiceAccessToken,
            TestContext.Current.CancellationToken);

        issueResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await _client.SendAsync(
            HttpMethod.Get,
            $"{BaseUrl}/{invoice.Id}",
            _invoiceAccessToken,
            TestContext.Current.CancellationToken);
        var issuedInvoice = await getResponse.Content.ReadFromJsonAsync<InvoiceDto>(TestContext.Current.CancellationToken);
        issuedInvoice!.Status.Should().Be(InvoiceStatus.Issued);
        issuedInvoice.Number.Should().NotBeNullOrEmpty();
    }

    private async Task<InvoiceDto> CreateInvoiceAsync()
    {
        var response = await _client.SendAsync(
            HttpMethod.Post,
            BaseUrl,
            _invoiceAccessToken,
            new CreateInvoiceRequest("CUST001", "Integration test invoice"),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<InvoiceDto>(TestContext.Current.CancellationToken))!;
    }

    private Task<HttpResponseMessage> AddLineAsync(Guid invoiceId, string productCode) =>
        _client.SendAsync(
            HttpMethod.Post,
            $"{BaseUrl}/{invoiceId}/line",
            _invoiceAccessToken,
            new AddInvoiceLineRequest(productCode, 2m, null),
            TestContext.Current.CancellationToken,
            idempotencyKey: Guid.CreateVersion7().ToString());

    private string ResolveUrl(string url) => url
        .Replace("{tenantId}", _tenantId.ToString())
        .Replace("{invoiceId}", Guid.CreateVersion7().ToString());
}
