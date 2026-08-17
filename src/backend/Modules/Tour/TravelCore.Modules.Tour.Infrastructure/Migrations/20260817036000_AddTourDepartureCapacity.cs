using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDepartureCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "maximum_pax",
                schema: "tour",
                table: "tour_departures",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "minimum_pax",
                schema: "tour",
                table: "tour_departures",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "maximum_pax",
                schema: "tour",
                table: "tour_departures");

            migrationBuilder.DropColumn(
                name: "minimum_pax",
                schema: "tour",
                table: "tour_departures");
        }
    }
}
