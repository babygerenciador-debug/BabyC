using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    public partial class RemoveSchedulesAndAddTripDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "fk_trips_schedules_schedule_id", table: "Trips");
            migrationBuilder.DropIndex(name: "ix_trips_schedule_id", table: "Trips");
            migrationBuilder.DropColumn(name: "schedule_id", table: "Trips");
            migrationBuilder.AddColumn<DateTime>(name: "scheduled_start_date", table: "Trips", type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.AddColumn<DateTime>(name: "scheduled_end_date", table: "Trips", type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.DropTable(name: "Schedules");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "scheduled_start_date", table: "Trips");
            migrationBuilder.DropColumn(name: "scheduled_end_date", table: "Trips");
            migrationBuilder.AddColumn<Guid>(name: "schedule_id", table: "Trips", type: "uuid", nullable: true);
            migrationBuilder.CreateIndex(name: "ix_trips_schedule_id", table: "Trips", column: "schedule_id");
            migrationBuilder.AddForeignKey(name: "fk_trips_schedules_schedule_id", table: "Trips", column: "schedule_id", principalTable: "Schedules", principalColumn: "id");
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    destination = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                constraints: table => { table.PrimaryKey("pk_schedules", x => x.id); });
        }
    }
}
