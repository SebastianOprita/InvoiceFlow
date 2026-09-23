using InvoiceFlow.Common.Application;
using Microsoft.AspNetCore.Builder;

namespace InvoiceFlow.Customers.Application;

public static class ConfigureServices
{
    public static WebApplicationBuilder RegisterApplicationServices(this WebApplicationBuilder builder)
    {
        return builder.RegisterCommonApplicationServices(typeof(ConfigureServices).Assembly);
    }
}
