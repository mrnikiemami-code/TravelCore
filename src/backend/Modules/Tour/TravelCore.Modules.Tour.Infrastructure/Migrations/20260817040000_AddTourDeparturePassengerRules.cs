using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDeparturePassengerRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_departure_passenger_rules",
                schema: "tour",
                columns: table => new
                {
                    tour_departure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    minimum_adults = table.Column<int>(type: "integer", nullable: false),
                    child_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    infant_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    maximum_passengers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_departure_passenger_rules", x => x.tour_departure_id);
                    table.ForeignKey(
                        name: "FK_tour_departure_passenger_rules_tour_departures_tour_departu~",
                        column: x => x.tour_departure_id,
                        principalSchema: "tour",
                        principalTable: "tour_departures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_departure_passenger_rules",
                schema: "tour");
        }
    }
}
