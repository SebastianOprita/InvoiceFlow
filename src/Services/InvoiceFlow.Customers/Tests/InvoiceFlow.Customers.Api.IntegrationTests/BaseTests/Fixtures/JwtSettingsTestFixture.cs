using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

public sealed class JwtSettingsTestFixture
{
    public IConfigurationRoot Configuration { get; }

    public JwtSettingsTestFixture()
    {
        var jwtSecret = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));

        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = jwtSecret
            })
            .Build();
    }
}
