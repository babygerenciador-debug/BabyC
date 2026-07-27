using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    public partial class AddFinancialMonths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<Guid>(
                name: "financial_month_id",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

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
