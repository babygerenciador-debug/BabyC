using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDriverAndVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "vehicles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "antt_expiry",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "artesp_expiry",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "brand",
                table: "vehicles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_odometer_km",
                table: "vehicles",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "fuel_alert_days",
                table: "vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fuel_alert_mode",
                table: "vehicles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "insurance_expiry",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_fuel_at",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "licensing_expiry",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nickname",
                table: "vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "vehicles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_vehicle_id",
                table: "drivers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_available",
                table: "drivers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "drivers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "drivers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tenant_id_status",
                table: "vehicles",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_drivers_assigned_vehicle_id",
                table: "drivers",
                column: "assigned_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_tenant_id_status",
                table: "drivers",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_vehicles_tenant_id_status",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_drivers_assigned_vehicle_id",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "ix_drivers_tenant_id_status",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "antt_expiry",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "artesp_expiry",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "brand",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "color",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "current_odometer_km",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "fuel_alert_days",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "fuel_alert_mode",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "insurance_expiry",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "last_fuel_at",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "licensing_expiry",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "nickname",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "assigned_vehicle_id",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "is_available",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "drivers");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
