using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

public sealed class ServiceClientExtensionsTests
{
    private static IConfiguration CreateConfiguration(
        string tokenEndpoint = "https://identity.test/connect/token",
        string clientId = "invoices-service",
        string clientSecret = "test-secret",
        string grantType = "client_credentials",
        string refreshSkew = "00:00:30",
        string requestTimeout = "00:00:15")
    {
        var values = new Dictionary<string, string?>
        {
            ["ServiceClient:TokenEndpoint"] = tokenEndpoint,
            ["ServiceClient:ClientId"] = clientId,
            ["ServiceClient:ClientSecret"] = clientSecret,
            ["ServiceClient:GrantType"] = grantType,
            ["ServiceClient:RefreshSkew"] = refreshSkew,
            ["ServiceClient:RequestTimeout"] = requestTimeout
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static ServiceProvider CreateProvider(
        IConfiguration? configuration = null)
    {
        var services = new ServiceCollection();

        services.AddServiceClientAuthentication(
            configuration ?? CreateConfiguration());

        return services.BuildServiceProvider(
            validateScopes: true);
    }

    [Fact]
    public void AddServiceClientAuthentication_ShouldRegisterRequiredServices()
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var tokenProvider =
            provider.GetRequiredService<IAccessTokenProvider>();

        var tokenEndpointClient =
            provider.GetRequiredService<ITokenEndpointClient>();

        var cache =
            provider.GetRequiredService<IAccessTokenCache>();

        var timeProvider =
            provider.GetRequiredService<TimeProvider>();

        // Assert
        Assert.IsType<AccessTokenProvider>(tokenProvider);
        Assert.IsType<HttpTokenEndpointClient>(tokenEndpointClient);
        Assert.IsType<MemoryAccessTokenCache>(cache);
        Assert.Same(TimeProvider.System, timeProvider);
    }

    [Fact]
    public void AddServiceClientAuthentication_ShouldRegisterSingletons()
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var tokenProvider1 =
            provider.GetRequiredService<IAccessTokenProvider>();

        var tokenProvider2 =
            provider.GetRequiredService<IAccessTokenProvider>();

        var endpoint1 =
            provider.GetRequiredService<ITokenEndpointClient>();

        var endpoint2 =
            provider.GetRequiredService<ITokenEndpointClient>();

        var cache1 =
            provider.GetRequiredService<IAccessTokenCache>();

        var cache2 =
            provider.GetRequiredService<IAccessTokenCache>();

        // Assert
        Assert.Same(tokenProvider1, tokenProvider2);
        Assert.Same(endpoint1, endpoint2);
        Assert.Same(cache1, cache2);
    }

    [Fact]
    public void AddServiceClientAuthentication_ShouldBindOptions()
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var options = provider
            .GetRequiredService<IOptions<ServiceTokenOptions>>()
            .Value;

        // Assert
        Assert.Equal(
            "https://identity.test/connect/token",
            options.TokenEndpoint);

        Assert.Equal("invoices-service", options.ClientId);
        Assert.Equal("test-secret", options.ClientSecret);
        Assert.Equal("client_credentials", options.GrantType);
        Assert.Equal(
            TimeSpan.FromSeconds(30),
            options.RefreshSkew);
        Assert.Equal(
            TimeSpan.FromSeconds(15),
            options.RequestTimeout);
    }

    [Fact]
    public void AddServiceClientAuthentication_ShouldConfigureTokenEndpointTimeout()
    {
        // Arrange
        using var provider = CreateProvider();

        var factory =
            provider.GetRequiredService<IHttpClientFactory>();

        // Act
        using var client = factory.CreateClient(
            HttpTokenEndpointClient.HttpClientName);

        // Assert
        Assert.Equal(
            TimeSpan.FromSeconds(15),
            client.Timeout);
    }

    [Fact]
    public void AddServiceClientAuthentication_ShouldRegisterHttpClientFactory()
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var factory =
            provider.GetService<IHttpClientFactory>();

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddServiceClientAuthentication_ShouldDisableRedirectsAndCookies()
    {
        // Arrange
        using var provider = CreateProvider();

        var factory =
            provider.GetRequiredService<IHttpMessageHandlerFactory>();

        // Act
        var handler = factory.CreateHandler(
            HttpTokenEndpointClient.HttpClientName);

        // Assert
        var socketsHandler = FindSocketsHttpHandler(handler);

        Assert.NotNull(socketsHandler);
        Assert.False(socketsHandler.AllowAutoRedirect);
        Assert.False(socketsHandler.UseCookies);
        Assert.Equal(
            TimeSpan.FromMinutes(5),
            socketsHandler.PooledConnectionLifetime);
    }

    [Fact]
    public void AddServiceClient_ShouldConfigureTypedClient()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddServiceClient<TestCustomerService>(
            new Uri("https://customers.test/"),
            TimeSpan.FromSeconds(30));

        using var provider = services.BuildServiceProvider();

        // Act
        var customerService =
            provider.GetRequiredService<TestCustomerService>();

        // Assert
        Assert.Equal(
            new Uri("https://customers.test/"),
            customerService.HttpClient.BaseAddress);

        Assert.Equal(
            TimeSpan.FromSeconds(30),
            customerService.HttpClient.Timeout);
    }

    [Fact]
    public void AddServiceClient_ShouldAllowMultipleTypedClients()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddServiceClient<TestCustomerService>(
            new Uri("https://customers.test/"),
            TimeSpan.FromSeconds(30));

        services.AddServiceClient<TestProductService>(
            new Uri("https://products.test/"),
            TimeSpan.FromSeconds(45));

        using var provider = services.BuildServiceProvider();

        // Act
        var customers =
            provider.GetRequiredService<TestCustomerService>();

        var products =
            provider.GetRequiredService<TestProductService>();

        // Assert
        Assert.Equal(
            "https://customers.test/",
            customers.HttpClient.BaseAddress?.AbsoluteUri);

        Assert.Equal(
            "https://products.test/",
            products.HttpClient.BaseAddress?.AbsoluteUri);

        Assert.Equal(
            TimeSpan.FromSeconds(30),
            customers.HttpClient.Timeout);

        Assert.Equal(
            TimeSpan.FromSeconds(45),
            products.HttpClient.Timeout);
    }

    [Fact]
    public void AddServiceClient_ShouldDisableRedirectsAndCookies()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddServiceClient<TestCustomerService>(
            new Uri("https://customers.test/"),
            TimeSpan.FromSeconds(30));

        using var provider = services.BuildServiceProvider();

        var factory =
            provider.GetRequiredService<IHttpMessageHandlerFactory>();

        // Act
        var handler = factory.CreateHandler(
            typeof(TestCustomerService).Name);

        // Assert
        var socketsHandler = FindSocketsHttpHandler(handler);

        Assert.NotNull(socketsHandler);
        Assert.False(socketsHandler.AllowAutoRedirect);
        Assert.False(socketsHandler.UseCookies);
        Assert.Equal(
            TimeSpan.FromMinutes(5),
            socketsHandler.PooledConnectionLifetime);
    }

    [Theory]
    [InlineData("http://customers.test/")]
    [InlineData("ftp://customers.test/")]
    public void AddServiceClient_WhenBaseAddressIsNotHttps_ShouldThrow(
        string address)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var action = () => services.AddServiceClient<TestCustomerService>(
            new Uri(address),
            TimeSpan.FromSeconds(30));

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData("https://customers.test/api")]
    [InlineData("https://customers.test/api?x=1")]
    [InlineData("https://customers.test/api#fragment")]
    public void AddServiceClient_WhenBaseAddressInvalid_ShouldThrow(
        string address)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var action = () => services.AddServiceClient<TestCustomerService>(
            new Uri(address),
            TimeSpan.FromSeconds(30));

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddServiceClient_WhenTimeoutInvalid_ShouldThrow(
        int seconds)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var action = () => services.AddServiceClient<TestCustomerService>(
            new Uri("https://customers.test/"),
            TimeSpan.FromSeconds(seconds));

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Theory]
    [InlineData("http://identity.test/connect/token")]
    [InlineData("ftp://identity.test/connect/token")]
    [InlineData("not-a-uri")]
    public void AddServiceClientAuthentication_WhenTokenEndpointInvalid_ShouldFailValidation(
        string endpoint)
    {
        // Arrange
        using var provider = CreateProvider(
            CreateConfiguration(tokenEndpoint: endpoint));

        // Act
        var action = () => ValidateStartup(provider);

        // Assert
        Assert.Throws<OptionsValidationException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddServiceClientAuthentication_WhenClientIdMissing_ShouldFailValidation(
        string clientId)
    {
        using var provider = CreateProvider(
            CreateConfiguration(clientId: clientId));

        Assert.Throws<OptionsValidationException>(
            () => ValidateStartup(provider));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddServiceClientAuthentication_WhenClientSecretMissing_ShouldFailValidation(
        string clientSecret)
    {
        using var provider = CreateProvider(
            CreateConfiguration(clientSecret: clientSecret));

        Assert.Throws<OptionsValidationException>(
            () => ValidateStartup(provider));
    }

    [Fact]
    public void AddServiceClientAuthentication_WhenRefreshSkewNegative_ShouldFailValidation()
    {
        using var provider = CreateProvider(
            CreateConfiguration(refreshSkew: "-00:00:01"));

        Assert.Throws<OptionsValidationException>(
            () => ValidateStartup(provider));
    }

    [Theory]
    [InlineData("00:00:00")]
    [InlineData("-00:00:01")]
    public void AddServiceClientAuthentication_WhenRequestTimeoutInvalid_ShouldFailValidation(
        string timeout)
    {
        using var provider = CreateProvider(
            CreateConfiguration(requestTimeout: timeout));

        Assert.Throws<OptionsValidationException>(
            () => ValidateStartup(provider));
    }

    private static void ValidateStartup(IServiceProvider provider)
    {
        provider.GetRequiredService<IStartupValidator>()
            .Validate();
    }

    private static SocketsHttpHandler? FindSocketsHttpHandler(
        HttpMessageHandler handler)
    {
        while (handler is DelegatingHandler delegatingHandler)
        {
            handler = delegatingHandler.InnerHandler!;
        }

        return handler as SocketsHttpHandler;
    }

    private sealed class TestCustomerService(HttpClient httpClient)
    {
        public HttpClient HttpClient { get; } = httpClient;
    }

    private sealed class TestProductService(HttpClient httpClient)
    {
        public HttpClient HttpClient { get; } = httpClient;
    }
}
