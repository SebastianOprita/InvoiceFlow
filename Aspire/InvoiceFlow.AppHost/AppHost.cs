using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", "Sqlpassword123!");
var sql = builder
    .AddSqlServer("sql", sqlPassword);

var customersDb = sql.AddDatabase("CustomersDb");
var identityDb = sql.AddDatabase("IdentityDb");
var invoicesDb = sql.AddDatabase("InvoicesDb");
var paymentsDb = sql.AddDatabase("PaymentsDb");
var productsDb = sql.AddDatabase("ProductsDb");

var jwtSigningKey = builder.Configuration["JwtSettings:Secret"];

if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "JwtSettings:Secret is not configured. Set it via AppHost secrets.");
    }

    jwtSigningKey = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(64));
}

var identityService = builder.AddProject<Projects.InvoiceFlow_Identity_Api>("invoiceflow-identity-api")
    .WithReference(identityDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(identityDb);

var customersService = builder.AddProject<Projects.InvoiceFlow_Customers_Api>("invoiceflow-customers-api")
    .WithReference(customersDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(customersDb);

var productsService = builder.AddProject<Projects.InvoiceFlow_Products_Api>("invoiceflow-products-api")
    .WithReference(productsDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(productsDb);

var invoicesService = builder.AddProject<Projects.InvoiceFlow_Invoices_Api>("invoiceflow-invoices-api")
    .WithReference(invoicesDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(invoicesDb);

var paymentsService = builder.AddProject<Projects.InvoiceFlow_Payments_Api>("invoiceflow-payments-api")
    .WithReference(paymentsDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(paymentsDb);

var gateway = builder
    .AddProject<Projects.InvoiceFlow_Gateway>("gateway")
    .WithReference(identityService)
    .WithReference(customersService)
    .WithReference(invoicesService)
    .WithReference(paymentsService)
    .WithReference(productsService)
    .WaitFor(identityService)
    .WaitFor(customersService)
    .WaitFor(invoicesService)
    .WaitFor(paymentsService)
    .WaitFor(productsService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
