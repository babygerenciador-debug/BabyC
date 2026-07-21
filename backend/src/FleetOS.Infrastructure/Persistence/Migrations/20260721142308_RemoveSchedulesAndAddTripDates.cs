using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSchedulesAndAddTripDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_trips_schedules_schedule_id",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropIndex(
                name: "ix_trips_schedule_id",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "schedule_id",
                table: "Trips");

            migrationBuilder.AlterColumn<Guid>(
                name: "driver_id",
                table: "VehicleIssueReports",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_end_date",
                table: "Trips",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_start_date",
                table: "Trips",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scheduled_end_date",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "scheduled_start_date",
                table: "Trips");

            migrationBuilder.AlterColumn<Guid>(
                name: "driver_id",
                table: "VehicleIssueReports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "schedule_id",
                table: "Trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    destination = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<long>(type: "bigint", nullable: false),
                    scheduled_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_schedules_drivers_driver_id",
                        column: x => x.driver_id,
                        principalTable: "drivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_schedules_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trips_schedule_id",
                table: "Trips",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_schedules_driver_id",
                table: "Schedules",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_schedules_vehicle_id",
                table: "Schedules",
                column: "vehicle_id");

            migrationBuilder.AddForeignKey(
                name: "fk_trips_schedules_schedule_id",
                table: "Trips",
                column: "schedule_id",
                principalTable: "Schedules",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
