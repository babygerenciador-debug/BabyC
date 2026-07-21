using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriversAndVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_vehicles_tenant_id_plate",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_drivers_tenant_id_cpf",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "ix_drivers_user_id",
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
                name: "chassis",
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
                name: "plate",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "address",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "assigned_vehicle_id",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "cnh_expiry",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "cnh_file_url",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "cpf",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "cpf_last4",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "email",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "name",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "drivers");

            migrationBuilder.RenameColumn(
                name: "color",
                table: "vehicles",
                newName: "antt_number");

            migrationBuilder.AlterColumn<int>(
                name: "year",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "model",
                table: "vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "chassi",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "license_plate",
                table: "vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "cnh_number",
                table: "drivers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cnh_category",
                table: "drivers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cnh_expiration_date",
                table: "drivers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "vehicle_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_documents_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tenant_id_chassi",
                table: "vehicles",
                columns: new[] { "tenant_id", "chassi" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tenant_id_license_plate",
                table: "vehicles",
                columns: new[] { "tenant_id", "license_plate" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_tenant_id_cnh_number",
                table: "drivers",
                columns: new[] { "tenant_id", "cnh_number" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_user_id",
                table: "drivers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_documents_vehicle_id",
                table: "vehicle_documents",
                column: "vehicle_id");

            migrationBuilder.AddForeignKey(
                name: "fk_drivers_users_user_id",
                table: "drivers",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_drivers_users_user_id",
                table: "drivers");

            migrationBuilder.DropTable(
                name: "vehicle_documents");

            migrationBuilder.DropIndex(
                name: "ix_vehicles_tenant_id_chassi",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_vehicles_tenant_id_license_plate",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "ix_drivers_tenant_id_cnh_number",
                table: "drivers");

            migrationBuilder.DropIndex(
                name: "ix_drivers_user_id",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "chassi",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "license_plate",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "cnh_expiration_date",
                table: "drivers");

            migrationBuilder.RenameColumn(
                name: "antt_number",
                table: "vehicles",
                newName: "color");

            migrationBuilder.AlterColumn<int>(
                name: "year",
                table: "vehicles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "model",
                table: "vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "antt_expiry",
                table: "vehicles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "artesp_expiry",
                table: "vehicles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "brand",
                table: "vehicles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chassis",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "current_odometer_km",
                table: "vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "fuel_alert_days",
                table: "vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fuel_alert_mode",
                table: "vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "insurance_expiry",
                table: "vehicles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_fuel_at",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "licensing_expiry",
                table: "vehicles",
                type: "date",
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

            migrationBuilder.AddColumn<string>(
                name: "plate",
                table: "vehicles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "cnh_number",
                table: "drivers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "cnh_category",
                table: "drivers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "drivers",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_vehicle_id",
                table: "drivers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "birth_date",
                table: "drivers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "cnh_expiry",
                table: "drivers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cnh_file_url",
                table: "drivers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                table: "drivers",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cpf_last4",
                table: "drivers",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "drivers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "drivers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "drivers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "drivers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_tenant_id_plate",
                table: "vehicles",
                columns: new[] { "tenant_id", "plate" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_tenant_id_cpf",
                table: "drivers",
                columns: new[] { "tenant_id", "cpf" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_drivers_user_id",
                table: "drivers",
                column: "user_id",
                unique: true);
        }
    }
}
