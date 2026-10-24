namespace InvoiceFlow.BuildingBlocks.Authorization;

[Flags]
public enum SystemPermissions : long
{
    None = 0,

    // Users
    UserRead = 1L << 0,
    UserCreate = 1L << 1,
    UserUpdate = 1L << 2,
    UserStatusManage = 1L << 3,

    // Roles
    RoleRead = 1L << 4,
    RoleCreate = 1L << 5,
    RoleUpdate = 1L << 6,
    RoleStatusManage = 1L << 7,


    // Permissions
    UserRoleManage = 1L << 8,
    RolePermissionsUpdate = 1L << 9,

    // Reports
    ReportRead = 1L << 10,

    // Customers
    CustomerRead = 1L << 11,
    CustomerCreate = 1L << 12,
    CustomerUpdate = 1L << 13,
    CustomerStatusManage = 1L << 14,

    // Invoices
    InvoiceRead = 1L << 15,
    InvoiceCreate = 1L << 16,
    InvoiceUpdate = 1L << 17,
    InvoiceStatusManage = 1L << 18,
    InvoiceIssue = 1L << 19,
    InvoiceCancel = 1L << 20,

    // Products
    ProductRead = 1L << 21,
    ProductCreate = 1L << 22,
    ProductUpdate = 1L << 23,
    ProductStatusManage = 1L << 24,

    // Payments
    PaymentRead = 1L << 25,
    PaymentCreate = 1L << 26,
    PaymentUpdate = 1L << 27,
    PaymentStatusManage = 1L << 28,
    PaymentPost = 1L << 29,
    PaymentCancel = 1L << 30,
}
