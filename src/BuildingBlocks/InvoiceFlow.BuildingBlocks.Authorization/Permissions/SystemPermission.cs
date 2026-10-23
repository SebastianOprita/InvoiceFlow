namespace InvoiceFlow.BuildingBlocks.Authorization;

[Flags]
public enum SystemPermissions : long
{
    None = 0,

    // Administration
    ManageUsers = 1L << 0,
    ManageRoles = 1L << 1,

    // Permissions
    UserRoleAssign = 1L << 2,
    RolePermissionsUpdate = 1L << 3,

    // Reports
    ReportRead = 1L << 4,

    // Customers
    CustomerRead = 1L << 5,
    CustomerCreate = 1L << 6,
    CustomerUpdate = 1L << 7,
    CustomerDelete = 1L << 8,

    // Invoices
    InvoiceRead = 1L << 9,
    InvoiceCreate = 1L << 10,
    InvoiceUpdate = 1L << 11,
    InvoiceDelete = 1L << 12,
    InvoiceIssue = 1L << 13,
    InvoiceCancel = 1L << 14,

    // Products
    ProductRead = 1L << 15,
    ProductCreate = 1L << 16,
    ProductUpdate = 1L << 17,
    ProductDelete = 1L << 18,

    // Payments
    PaymentRead = 1L << 19,
    PaymentCreate = 1L << 20,
    PaymentUpdate = 1L << 21,
    PaymentDelete = 1L << 22,
    PaymentPost = 1L << 23,
    PaymentCancel = 1L << 24,
}
