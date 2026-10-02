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

builder.AddProject<Projects.InvoiceFlow_Identity_Api>("invoiceflow-identity-api")
    .WithReference(identityDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(identityDb);

builder.AddProject<Projects.InvoiceFlow_Customers_Api>("invoiceflow-customers-api")
    .WithReference(customersDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(customersDb);

builder.AddProject<Projects.InvoiceFlow_Invoices_Api>("invoiceflow-invoices-api")
    .WithReference(invoicesDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(invoicesDb);

builder.AddProject<Projects.InvoiceFlow_Payments_Api>("invoiceflow-payments-api")
    .WithReference(paymentsDb)
    .WithEnvironment("JwtSettings__Secret", jwtSigningKey)
    .WaitFor(paymentsDb);

builder.Build().Run();
