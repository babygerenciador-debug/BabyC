using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockBalanceUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_stock_balances_tenant_id_product_id",
                table: "StockBalances",
                columns: new[] { "tenant_id", "product_id" },
                unique: true,
                filter: "\"location_type\" = 1 AND \"vehicle_id\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stock_balances_tenant_id_product_id_vehicle_id",
                table: "StockBalances",
                columns: new[] { "tenant_id", "product_id", "vehicle_id" },
                unique: true,
                filter: "\"location_type\" = 2 AND \"vehicle_id\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stock_balances_tenant_id_product_id",
                table: "StockBalances");

            migrationBuilder.DropIndex(
                name: "ix_stock_balances_tenant_id_product_id_vehicle_id",
                table: "StockBalances");
        }
    }
}
