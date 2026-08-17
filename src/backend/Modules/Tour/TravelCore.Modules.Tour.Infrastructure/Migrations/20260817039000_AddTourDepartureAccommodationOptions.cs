using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDepartureAccommodationOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_departure_accommodation_options",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_departure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nights = table.Column<int>(type: "integer", nullable: false),
                    board_type = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_departure_accommodation_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_departure_accommodation_options_tour_departures_tour_d~",
                        column: x => x.tour_departure_id,
                        principalSchema: "tour",
                        principalTable: "tour_departures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_departure_accommodation_options_departure_id",
                schema: "tour",
                table: "tour_departure_accommodation_options",
                column: "tour_departure_id");

            migrationBuilder.CreateIndex(
                name: "ix_tour_departure_accommodation_options_place_id",
                schema: "tour",
                table: "tour_departure_accommodation_options",
                column: "place_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_departure_accommodation_options",
                schema: "tour");
        }
    }
}
