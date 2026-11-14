using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;

public sealed class JwtSettingsTestFixture
{
    public IConfigurationRoot Configuration { get; }

    public JwtSettingsTestFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            })
            .Build();
    }
}
