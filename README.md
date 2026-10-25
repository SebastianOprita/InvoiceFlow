## Entity Framework Core Migrations

Each microservice manages its own database schema and migrations independently using Entity Framework Core. Migrations are stored in the respective service's Infrastructure project under the `Migrations` folder.

Run the following commands from the solution root directory (`C:\Dev\InvoiceFlow`).

### Customers Service

```cmd
dotnet ef migrations add InitialMigration --project "src\Services\InvoiceFlow.Customers\InvoiceFlow.Customers.Infrastructure\InvoiceFlow.Customers.Infrastructure.csproj" --startup-project "src\Services\InvoiceFlow.Customers\InvoiceFlow.Customers.Api\InvoiceFlow.Customers.Api.csproj" --context CustomersDbContext --output-dir Migrations
```

### Identity Service

```cmd
dotnet ef migrations add InitialMigration --project "src\Services\InvoiceFlow.Identity\InvoiceFlow.Identity.Infrastructure\InvoiceFlow.Identity.Infrastructure.csproj" --startup-project "src\Services\InvoiceFlow.Identity\InvoiceFlow.Identity.Api\InvoiceFlow.Identity.Api.csproj" --context IdentityDbContext --output-dir Migrations
```

### Product Service

```cmd
dotnet ef migrations add InitialMigration --project "src\Services\InvoiceFlow.Products\InvoiceFlow.Products.Infrastructure\InvoiceFlow.Products.Infrastructure.csproj" --startup-project "src\Services\InvoiceFlow.Products\InvoiceFlow.Products.Api\InvoiceFlow.Products.Api.csproj" --context ProductsDbContext --output-dir Migrations
```

### Notes

- Each service uses `IDesignTimeDbContextFactory<TContext>` to create its DbContext without starting the API.
- Replace `InitialMigration` with a descriptive name for subsequent schema changes.
- Generating migrations does not apply them to the database.
- Use `dotnet ef database update` with the appropriate project, startup project, and DbContext to apply migrations.

CreateInvoice
  -> Invoicing saves Invoice + Outbox
  -> publishes InvoiceIssued

InvoiceIssued
  -> Notifications sends "invoice issued" email
  -> Reporting projects dashboard row
  -> Reminder schedules due-soon reminder

RecordPayment
  -> Payments saves Payment + PaymentAllocations + Outbox
  -> publishes PaymentRecorded
  -> publishes PaymentAllocated (one event per invoice allocation)

PaymentAllocated
  -> Invoicing recalculates outstanding balance
  -> publishes InvoicePartiallyPaid or InvoicePaid

InvoicePaid / InvoicePartiallyPaid
  -> Notifications sends status email
  -> Reporting updates read model


InvoiceFlow.Identity Service
	Users
	Roles
	RefreshTokens

InvoiceFlow.Invoicing Service
	Invoices
	InvoiceItems
	InvoiceStatusHistory

InvoiceFlow.Payments Service
	Payments
	PaymentAllocations
	Refunds

InvoiceFlow.Customers Service
    Customers
    CustomerContacts
    CustomerAddresses

InvoiceFlow.Notifications Service
    NotificationTemplates
    NotificationLogs
    DeliveryAttempts

InvoiceFlow.Reminder Service
	start as a worker, not a domain database

InvoiceFlow.Reporting Service
	read-model tables only
	projections from events
	
	
Example event flow
	User creates invoice in Invoice Service
	Invoice Service publishes InvoiceIssued
	Notification Service consumes it and sends email
	Reporting Service consumes it and updates dashboard
	Reminder Service later detects due date is close and publishes InvoiceDueSoon
	Notification Service sends reminder
	Payment Service records payment and publishes PaymentRecorded
	Invoice Service consumes it and updates invoice status to Paid or PartiallyPaid
	Notification and Reporting also react


Do not start with separate services for every tiny thing like:
	Tax Service
	PDF Service
	Audit Service
	Template Service
Those can be modules inside existing services first.


## License

InvoiceFlow is source-available under the
[PolyForm Noncommercial License 1.0.0](LICENSE).

You may use, study, modify, and distribute this software for permitted
non-commercial purposes in accordance with the license.

Commercial use is not permitted under this license.

For commercial licensing or additional permissions, please contact the
copyright holder.

Copyright © 2026 Sebastian Oprita.
