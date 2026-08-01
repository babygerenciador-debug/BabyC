using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefineChecklists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "business_unit_id",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "category",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "DailyChecklistItems");

            migrationBuilder.DropColumn(
                name: "category",
                table: "ChecklistItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "business_unit_id",
                table: "DailyChecklistItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "DailyChecklistItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "DailyChecklistItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "DailyChecklistItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "DailyChecklistItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "DailyChecklistItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "DailyChecklistItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "row_version",
                table: "DailyChecklistItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "DailyChecklistItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "DailyChecklistItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "DailyChecklistItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "ChecklistItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
