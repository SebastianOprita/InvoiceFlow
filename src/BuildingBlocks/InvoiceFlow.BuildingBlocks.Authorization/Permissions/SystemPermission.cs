namespace InvoiceFlow.BuildingBlocks.Authorization;

[Flags]
public enum SystemPermissions : long
{
    None = 0,

    // Administration
    ManageUsers = 1L << 0,
    ManageRoles = 1L << 1,

    // Reports
    ReportRead = 1L << 2,

    // Customers
    CustomerRead = 1L << 3,
    CustomerCreate = 1L << 4,
    CustomerUpdate = 1L << 5,
    CustomerDelete = 1L << 6,

    // Invoices
    InvoiceRead = 1L << 7,
    InvoiceCreate = 1L << 8,
    InvoiceUpdate = 1L << 9,
    InvoiceDelete = 1L << 10,

    // Payments
    PaymentRead = 1L << 11,
    PaymentCreate = 1L << 12,
}
