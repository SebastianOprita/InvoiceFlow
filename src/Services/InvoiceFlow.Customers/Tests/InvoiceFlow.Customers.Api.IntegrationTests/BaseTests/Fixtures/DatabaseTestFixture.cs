using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

public class DatabaseTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    public string ConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "CustomersIntegrationTests"
        };

        return builder.ConnectionString;
    }

    public DatabaseTestFixture()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04")
        .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
