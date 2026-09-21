var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", "Sqlpassword123!");
var sql = builder
    .AddSqlServer("sql", sqlPassword);

var customersDb = sql.AddDatabase("CustomersDb");
var identityDb = sql.AddDatabase("IdentityDb");
var invoicesDb = sql.AddDatabase("InvoicesDb");
var paymentsDb = sql.AddDatabase("PaymentsDb");

builder.AddProject<Projects.InvoiceFlow_Identity_Api>("invoiceflow-identity-api")
    .WithReference(identityDb)
    .WaitFor(identityDb);

builder.AddProject<Projects.InvoiceFlow_Customers_Api>("invoiceflow-customers-api")
    .WithReference(customersDb)
    .WaitFor(customersDb);

builder.AddProject<Projects.InvoiceFlow_Invoices_Api>("invoiceflow-invoices-api")
    .WithReference(invoicesDb)
    .WaitFor(invoicesDb);

builder.AddProject<Projects.InvoiceFlow_Payments_Api>("invoiceflow-payments-api")
    .WithReference(paymentsDb)
    .WaitFor(paymentsDb);

builder.Build().Run();
