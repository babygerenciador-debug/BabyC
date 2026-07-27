using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    public partial class AddFinancialMonths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create FinancialMonths table
            migrationBuilder.CreateTable(
                name: "FinancialMonths",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month_number = table.Column<int>(type: "integer", nullable: false),
                    owner_salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_financial_months", x => x.id);
                });

            // 2. Seed one initial FinancialMonth per existing tenant
            //    Must run before adding the FK so existing FinancialTransactions
            //    can reference a valid month.
            //    Uses raw SQL because EF migrations can't reference runtime data.
            migrationBuilder.Sql(@"
                INSERT INTO ""FinancialMonths"" (""id"", ""year"", ""month_number"", ""owner_salary"", ""status"", ""opened_at"", ""tenant_id"", ""organization_id"", ""business_unit_id"", ""created_at"", ""row_version"")
                SELECT
                    gen_random_uuid(),
                    EXTRACT(YEAR FROM NOW())::int,
                    EXTRACT(MONTH FROM NOW())::int,
                    COALESCE(t.""owner_salary"", 0),
                    0,
                    NOW(),
                    t.""id"",
                    COALESCE(o.""id"", '00000000-0000-0000-0000-000000000000'),
                    COALESCE(bu.""id"", '00000000-0000-0000-0000-000000000000'),
                    NOW(),
                    0
                FROM ""tenants"" t
                LEFT JOIN ""organizations"" o ON o.""tenant_id"" = t.""id"" AND o.""deleted_at"" IS NULL
                LEFT JOIN ""business_units"" bu ON bu.""tenant_id"" = t.""id"" AND bu.""deleted_at"" IS NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""FinancialMonths"" fm WHERE fm.""tenant_id"" = t.""id""
                )
            ");

            // 3. Add financial_month_id as nullable first (existing rows need a valid FK target)
            migrationBuilder.AddColumn<Guid>(
                name: "financial_month_id",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: true);

            // 4. Point existing FinancialTransactions to the seeded month for their tenant
            migrationBuilder.Sql(@"
                UPDATE ""FinancialTransactions"" ft
                SET ""financial_month_id"" = (
                    SELECT fm.""id"" FROM ""FinancialMonths"" fm
                    WHERE fm.""tenant_id"" = ft.""tenant_id""
                    ORDER BY fm.""year"" DESC, fm.""month_number"" DESC
                    LIMIT 1
                )
                WHERE ft.""financial_month_id"" IS NULL
            ");

            // 5. Make the column NOT NULL (now that every row has a value)
            //    EF Core's AlterColumn doesn't generate the correct SQL for Npgsql here
            //    so we use raw SQL.
            migrationBuilder.Sql(@"
                ALTER TABLE ""FinancialTransactions"" ALTER COLUMN ""financial_month_id"" SET NOT NULL
            ");

            // 6. Create index and FK
            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_financial_month_id",
                table: "FinancialTransactions",
                column: "financial_month_id");

            migrationBuilder.AddForeignKey(
                name: "fk_financial_transactions_financial_months_financial_month_id",
                table: "FinancialTransactions",
                column: "financial_month_id",
                principalTable: "FinancialMonths",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_financial_transactions_financial_months_financial_month_id",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "ix_financial_transactions_financial_month_id",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "financial_month_id",
                table: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "FinancialMonths");
        }
    }
}
