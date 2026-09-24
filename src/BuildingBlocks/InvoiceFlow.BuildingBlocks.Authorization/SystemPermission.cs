namespace InvoiceFlow.BuildingBlocks.Authorization;

[Flags]
public enum SystemPermission : long
{
    None = 0,

    // Administration
    ManageUsers = 1 << 0,
    ManageRoles = 1 << 1,

    // Reports
    ReportView = 1 << 2,

    // Customers
    CustomerView = 1 << 3,
    CustomerCreate = 1 << 4,
    CustomerUpdate = 1 << 5,
    CustomerDelete = 1 << 6,

    // Invoices
    InvoiceView = 1 << 7,
    InvoiceCreate = 1 << 8,
    InvoiceUpdate = 1 << 9,
    InvoiceDelete = 1 << 10,

    // Payments
    PaymentView = 1 << 11,
    PaymentCreate = 1 << 12,
}

public static class SystemPermissionExtensions
{
    public static readonly SystemPermission AllValidPermissions = Enum
        .GetValues<SystemPermission>()
        .Aggregate(SystemPermission.None, (acc, v) => acc | v);

    public static bool IsValid(this SystemPermission systemPermission)
    {
        return (systemPermission & ~AllValidPermissions) == 0;
    }
}
