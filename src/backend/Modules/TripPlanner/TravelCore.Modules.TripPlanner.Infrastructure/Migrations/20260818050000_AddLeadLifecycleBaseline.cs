using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.TripPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadLifecycleBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "status_changed_at",
                schema: "trip_planner",
                table: "leads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                schema: "trip_planner",
                table: "leads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status_changed_at",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "trip_planner",
                table: "leads");
        }
    }
}
