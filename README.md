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

InvoiceFlow.Notifications Service
	Customers
	CustomerContacts
	CustomerAddresses

InvoiceFlow.Customers Service
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


