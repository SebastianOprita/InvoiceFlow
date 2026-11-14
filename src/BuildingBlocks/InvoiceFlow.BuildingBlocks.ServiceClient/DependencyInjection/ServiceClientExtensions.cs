using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public static class ServiceClientExtensions
{
    private static readonly TimeSpan DefaultPooledConnectionLifetime =
        TimeSpan.FromMinutes(5);


    public static IServiceCollection AddServiceClientAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ServiceTokenOptions>()
            .Bind(configuration.GetSection(
                ServiceTokenOptions.SectionName))
            .Validate(
                options => IsHttpsUri(options.TokenEndpoint),
                "TokenEndpoint must be an absolute HTTPS URL.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ClientId),
                "ClientId is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ClientSecret),
                "ClientSecret is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.GrantType),
                "GrantType is required.")
            .Validate(
                options => options.RefreshSkew >= TimeSpan.Zero,
                "RefreshSkew cannot be negative.")
            .Validate(
                options => options.RequestTimeout > TimeSpan.Zero &&
                           options.RequestTimeout != Timeout.InfiniteTimeSpan,
                "RequestTimeout must be positive and finite.")
            .ValidateOnStart();

        services.AddMemoryCache();

        services.TryAddSingleton<TimeProvider>(
            TimeProvider.System);

        services.TryAddSingleton<
            IAccessTokenCache,
            MemoryAccessTokenCache>();

        services.TryAddSingleton<
            ITokenEndpointClient,
            HttpTokenEndpointClient>();

        services.TryAddSingleton<
            IAccessTokenProvider,
            AccessTokenProvider>();

        services.AddHttpClient(
            HttpTokenEndpointClient.HttpClientName,
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ServiceTokenOptions>>()
                    .Value;

                client.Timeout = options.RequestTimeout;
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                CreatePrimaryHandler());

        return services;
    }


    public static IHttpClientBuilder AddServiceClient<TClient>(
        this IServiceCollection services,
        Uri baseAddress,
        TimeSpan requestTimeout)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(baseAddress);

        if (!IsHttpsUri(baseAddress.AbsoluteUri))
        {
            throw new ArgumentException(
                "BaseAddress must be an absolute HTTPS URI.",
                nameof(baseAddress));
        }

        if (!baseAddress.AbsolutePath.EndsWith('/'))
        {
            throw new ArgumentException(
                "BaseAddress must end with '/'.",
                nameof(baseAddress));
        }

        if (!string.IsNullOrEmpty(baseAddress.Query))
        {
            throw new ArgumentException(
                "BaseAddress cannot contain a query string.",
                nameof(baseAddress));
        }


        if (requestTimeout <= TimeSpan.Zero ||
            requestTimeout == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestTimeout),
                "RequestTimeout must be positive and finite.");
        }


        return services.AddHttpClient<TClient>(client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = requestTimeout;
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
            CreatePrimaryHandler());
    }

    private static SocketsHttpHandler CreatePrimaryHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            PooledConnectionLifetime =
                DefaultPooledConnectionLifetime
        };
    }

    private static bool IsHttpsUri(string? value)
    {
        return Uri.TryCreate(
                   value,
                   UriKind.Absolute,
                   out var uri) &&
               uri.Scheme == Uri.UriSchemeHttps &&
               string.IsNullOrEmpty(uri.UserInfo) &&
               string.IsNullOrEmpty(uri.Fragment);
    }

    //private static bool IsValidServiceUri(string? value)
    //{
    //    return Uri.TryCreate(
    //               value,
    //               UriKind.Absolute,
    //               out var uri) &&
    //           (uri.Scheme == Uri.UriSchemeHttps ||
    //            uri.Scheme == "https+http");
    //}
}
