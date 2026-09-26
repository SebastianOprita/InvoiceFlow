using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Customers.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests;

[Collection(CustomersApiCollection.Name)]
public sealed class CustomersApiTests : IAsyncLifetime
{
    private readonly CustomersApiFactory _factory;
    private readonly TestClient _client;
    private readonly Guid _tenantId;
    private readonly string _baseUrl;
    private readonly string _customerAccessToken;

    public CustomersApiTests(CustomersApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _tenantId = Guid.CreateVersion7();
        _baseUrl = $"/api/{_tenantId}/customers/";
        _customerAccessToken = TestJwtTokenFactory.CreateAccessToken(
            _tenantId,
            SystemPermission.CustomerView |
            SystemPermission.CustomerCreate |
            SystemPermission.CustomerUpdate |
            SystemPermission.CustomerDelete);
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
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl, _customerAccessToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenCustomerExists_Should_ReturnOk()
    {
        // Arrange
        string customerCode = "TestCustomer", registrationNumber = "TestCustomer", taxNumber = "TestCustomer";
        var customerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + customerId, _customerAccessToken);

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
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + Guid.CreateVersion7(), _customerAccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
