using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDepartureStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "status",
                schema: "tour",
                table: "tour_departures",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "ix_tour_departures_status",
                schema: "tour",
                table: "tour_departures",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tour_departures_status",
                schema: "tour",
                table: "tour_departures");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "tour",
                table: "tour_departures");
        }
    }
}
