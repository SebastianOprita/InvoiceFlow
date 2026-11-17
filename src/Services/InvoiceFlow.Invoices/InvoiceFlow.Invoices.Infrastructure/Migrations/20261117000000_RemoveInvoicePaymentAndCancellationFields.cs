using System;
using InvoiceFlow.Invoices.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceFlow.Invoices.Infrastructure.Migrations;

[DbContext(typeof(InvoicesDbContext))]
[Migration("20261117000000_RemoveInvoicePaymentAndCancellationFields")]
public sealed class RemoveInvoicePaymentAndCancellationFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InvoicePaymentSummaries",
            columns: table => new
            {
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                PaidCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                OutstandingCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                PaymentBalanceVersion = table.Column<long>(type: "bigint", nullable: false),
                SourceUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastAppliedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InvoicePaymentSummaries", x => new { x.TenantId, x.InvoiceId });
            });

        migrationBuilder.DropIndex(
            name: "IX_Invoices_TenantId_PaymentStatus",
            table: "Invoices");

        migrationBuilder.DropColumn(name: "PaymentStatus", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaidAmount", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaidCurrencyCode", table: "Invoices");
        migrationBuilder.DropColumn(name: "CancelledAtUtc", table: "Invoices");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InvoicePaymentSummaries");

        migrationBuilder.AddColumn<decimal>(
            name: "PaidAmount",
            table: "Invoices",
            type: "decimal(19,4)",
            precision: 19,
            scale: 4,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "PaidCurrencyCode",
            table: "Invoices",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: true);

        migrationBuilder.Sql(
            "UPDATE [Invoices] SET [PaidCurrencyCode] = [CurrencyCode]");

        migrationBuilder.AlterColumn<string>(
            name: "PaidCurrencyCode",
            table: "Invoices",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3,
            oldNullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CancelledAtUtc",
            table: "Invoices",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PaymentStatus",
            table: "Invoices",
            type: "int",
            nullable: false,
            computedColumnSql: "CASE WHEN [PaidAmount] = 0 THEN 1 WHEN [PaidAmount] < [GrossAmount] THEN 2 ELSE 3 END",
            stored: true);

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_TenantId_PaymentStatus",
            table: "Invoices",
            columns: new[] { "TenantId", "PaymentStatus" });
    }
}
