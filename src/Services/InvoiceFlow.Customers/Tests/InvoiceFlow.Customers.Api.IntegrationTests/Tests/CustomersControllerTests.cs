using FluentAssertions;
using InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Customers.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests;

[Collection(CustomersApiCollection.Name)]
public sealed class CustomersControllerTests : IAsyncLifetime
{
    private readonly CustomersApiFactory _factory;
    private readonly TestClient _client;
    private readonly Guid _tenantId;
    private readonly string _baseUrl;

    public CustomersControllerTests(CustomersApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _tenantId = Guid.CreateVersion7();
        _baseUrl = $"/api/{_tenantId}/customers/";
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }


    [Fact]
    public async Task GetAll_Should_ReturnOk()
    {
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenCustomerExists_Should_ReturnOk()
    {
        // Arrange
        string customerCode = "Test5", registrationNumber = "Test5", taxNumber = "Test5";
        var customerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + customerId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(TestContext.Current.CancellationToken);

        customer.Should().NotBeNull();
        customer!.Id.Should().Be(customerId);
        customer.TenantId.Should().Be(_tenantId);
        customer.Name.Should().Be($"Customer-{customerId:N}");
        customer.CustomerCode.Should().Be(customerCode);
        customer.CustomerTaxDetails.RegistrationNumber.Should().Be(registrationNumber);
        customer.CustomerTaxDetails.TaxNumber.Should().Be(taxNumber);
    }

    [Fact]
    public async Task GetById_WhenCustomerDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + Guid.CreateVersion7());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
