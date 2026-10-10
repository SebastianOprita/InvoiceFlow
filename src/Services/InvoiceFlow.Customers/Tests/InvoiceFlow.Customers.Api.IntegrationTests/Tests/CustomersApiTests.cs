using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
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
            SystemPermission.CustomerDelete,
            _factory.JwtSettings);
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public static IEnumerable<object[]> ProtectedEndpoints =>
    [
        [HttpMethod.Get,    "/api/{tenantId}/customers"],
        [HttpMethod.Get,    "/api/{tenantId}/customers/{customerId}"],
        [HttpMethod.Post,   "/api/{tenantId}/customers"],
        [HttpMethod.Patch,  "/api/{tenantId}/customers/{customerId}"],
        [HttpMethod.Post,   "/api/{tenantId}/customers/activate/{customerId}"],
        [HttpMethod.Delete, "/api/{tenantId}/customers/deactivate/{customerId}"]
    ];

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        url = url
            .Replace("{tenantId}", _tenantId.ToString())
            .Replace("{customerId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(method, url, "");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_Should_Return_Forbidden_When_Not_Permissions(
        HttpMethod method,
        string url)
    {
        url = url
            .Replace("{tenantId}", _tenantId.ToString())
            .Replace("{customerId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(method, url, TestJwtTokenFactory.CreateAccessToken(_tenantId, SystemPermission.None, _factory.JwtSettings));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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

    [Fact]
    public async Task Create_Should_Return_Created()
    {
        // Arrange
        string customerCode = "Test", registrationNumber = "Test", taxNumber = "Test";

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl, _customerAccessToken, CreateValidCustomerRequest(customerCode, registrationNumber, taxNumber));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(TestContext.Current.CancellationToken);
        customer.Should().NotBeNull();
        customer.TenantId.Should().Be(_tenantId);
        customer.CustomerCode.Should().Be(customerCode);
        customer.CustomerTaxDetails.RegistrationNumber.Should().Be(registrationNumber);
        customer.CustomerTaxDetails.TaxNumber.Should().Be(taxNumber);
    }

    [Fact]
    public async Task Create_WhenCustomerCodeAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        string customerCode = "Test", registrationNumber = "Test", taxNumber = "Test";
        var createCustomerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        var request = CreateValidCustomerRequest(
            customerCode: customerCode,
            registrationNumber: "NewTest",
            taxNumber: "NewTest");

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl, _customerAccessToken, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.CustomerCodeAlreadyExists.Code);
    }

    [Fact]
    public async Task Create_WhenRegistrationNumberAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        string customerCode = "Test", registrationNumber = "Test", taxNumber = "Test";
        var createCustomerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        var request = CreateValidCustomerRequest(
            customerCode: "NewTest",
            registrationNumber: registrationNumber,
            taxNumber: "NewTest");

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl, _customerAccessToken, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain(ApplicationErrors.CustomerRegistrationNumberAlreadyExists.Code);
    }

    [Fact]
    public async Task Create_WhenTaxNumberAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        string customerCode = "Test", registrationNumber = "Test", taxNumber = "Test";
        var createCustomerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        var request = CreateValidCustomerRequest(
            customerCode: "NewTest",
            registrationNumber: "NewTest",
            taxNumber: taxNumber);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl, _customerAccessToken, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain(ApplicationErrors.CustomerTaxNumberAlreadyExists.Code);
    }

    [Fact]
    public async Task Update_WhenCustomerExists_Should_ReturnOk()
    {
        // Arrange
        string customerCode = "Test", registrationNumber = "Test", taxNumber = "Test";
        var createCustomerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        var updateRequest = new UpdateCustomerRequest(
            "Updated Customer",
            new CustomerContactRequest(
                "updated.customer@email.com",
                "00999999999"),
            new CustomerAddressRequest(
                "Updated Street",
                "Updated Number 10",
                "Cluj-Napoca",
                "Cluj",
                "Romania",
                "400000"),
            new CustomerCreditPolicyRequest(
                5000,
                45)
        );

        // Act
        var response = await _client.SendAsync(HttpMethod.Patch, _baseUrl + $"{createCustomerId}", _customerAccessToken, updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>(TestContext.Current.CancellationToken);

        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Id.Should().Be(createCustomerId);
        updatedCustomer.TenantId.Should().Be(_tenantId);
        updatedCustomer.Name.Should().Be("Updated Customer");
        updatedCustomer.CustomerContact.Email.Should().Be("updated.customer@email.com");
        updatedCustomer.CustomerContact.Phone.Should().Be("00999999999");
        updatedCustomer.CustomerAddress.City.Should().Be("Cluj-Napoca");
        updatedCustomer.CustomerCreditPolicy.CreditLimit.Should().Be(5000);
        updatedCustomer.CustomerCreditPolicy.PaymentTermDays.Should().Be(45);
    }

    [Fact]
    public async Task Update_WhenCustomerDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateCustomerRequest(
            "Updated Customer",
            new CustomerContactRequest(
                "updated.customer@email.com",
                "00999999999"),
            new CustomerAddressRequest(
                "Updated Street",
                "Updated Number 10",
                "Cluj-Napoca",
                "Cluj",
                "Romania",
                "400000"),
            new CustomerCreditPolicyRequest(
                5000,
                45)
        );

        // Act
        var response = await _client.SendAsync(HttpMethod.Patch, _baseUrl + $"{Guid.CreateVersion7()}", _customerAccessToken, updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenCustomerExists_Should_ReturnOk()
    {
        // Arrange
        string customerCode = "Test", registrationNumber = "Test", taxNumber = "Test";
        var createCustomerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, _baseUrl + $"deactivate/{createCustomerId}", _customerAccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await _client.SendAsync(HttpMethod.Get, _baseUrl + $"{createCustomerId}", _customerAccessToken);

        var updatedCustomer = await updatedResponse.Content.ReadFromJsonAsync<CustomerDto>(TestContext.Current.CancellationToken);

        updatedCustomer.Should().NotBeNull();
        updatedCustomer.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenCustomerDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, _baseUrl + $"deactivate/{Guid.CreateVersion7()}", _customerAccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WhenCustomerIsInactive_Should_ReturnOk()
    {
        // Arrange
        string customerCode = "Test8", registrationNumber = "Test8", taxNumber = "Test8";
        var createCustomerId = await _factory.InsertCustomerAsync(_tenantId, customerCode, registrationNumber, taxNumber);

        // deactivate first
        var deactivateResponse = await _client.SendAsync(HttpMethod.Delete, _baseUrl + $"deactivate/{createCustomerId}", _customerAccessToken);

        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var activateResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + $"activate/{createCustomerId}", _customerAccessToken);

        // Assert
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await _client.SendAsync(HttpMethod.Get, _baseUrl + $"{createCustomerId}", _customerAccessToken);

        var updatedCustomer = await updatedResponse.Content.ReadFromJsonAsync<CustomerDto>(TestContext.Current.CancellationToken);

        updatedCustomer.Should().NotBeNull();
        updatedCustomer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenCustomerDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + $"activate/{Guid.CreateVersion7()}", _customerAccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    private static CreateCustomerRequest CreateValidCustomerRequest(
        string customerCode,
        string registrationNumber,
        string? taxNumber)
    {
        return new CreateCustomerRequest
        (
            $"Customer-{Guid.CreateVersion7():N}",
            customerCode,
            new CustomerContactRequest(
                $"customer-{Guid.CreateVersion7():N}@email.com",
                "00767057577"),
            new CustomerTaxDetailsRequest(
                taxNumber,
                registrationNumber),
            new CustomerAddressRequest(
                "Strada Mihai Eminescu",
                "Numarul 2",
                "Bucharest",
                "Bucharest",
                "Romania",
                "010000"),
            "RON",
            new CustomerCreditPolicyRequest(
                1200,
                30)
        );
    }
}
