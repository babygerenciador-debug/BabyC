using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeVehiclePropertiesOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_drivers_assigned_vehicle_id",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "assigned_vehicle_id",
                table: "drivers");

            migrationBuilder.AlterColumn<int>(
                name: "year",
                table: "vehicles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "chassi",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "capacity",
                table: "vehicles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_driver_id",
                table: "vehicles",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assigned_driver_id",
                table: "vehicles");

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
                name: "chassi",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "capacity",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_vehicle_id",
                table: "drivers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_drivers_assigned_vehicle_id",
                table: "drivers",
                column: "assigned_vehicle_id");
        }
    }
}
