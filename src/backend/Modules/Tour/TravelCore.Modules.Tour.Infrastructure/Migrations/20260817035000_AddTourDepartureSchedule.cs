using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDepartureSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<LocalDate>(
                name: "end_date",
                schema: "tour",
                table: "tour_departures",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "start_date",
                schema: "tour",
                table: "tour_departures",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_zone_id",
                schema: "tour",
                table: "tour_departures",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_date",
                schema: "tour",
                table: "tour_departures");

            migrationBuilder.DropColumn(
                name: "start_date",
                schema: "tour",
                table: "tour_departures");

            migrationBuilder.DropColumn(
                name: "time_zone_id",
                schema: "tour",
                table: "tour_departures");
        }
    }
}
