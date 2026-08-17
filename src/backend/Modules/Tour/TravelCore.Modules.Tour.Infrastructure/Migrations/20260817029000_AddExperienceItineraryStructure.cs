using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceItineraryStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_experience_itineraries",
                schema: "tour",
                columns: table => new
                {
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_itineraries", x => x.tour_product_id);
                    table.ForeignKey(
                        name: "FK_tour_experience_itineraries_tour_experience_specializations_tour_product_id",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_specializations",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_experience_itinerary_days",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_itinerary_days", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_experience_itinerary_days_tour_experience_itineraries_tour_product_id",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_itineraries",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_experience_itinerary_stops",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itinerary_day_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_itinerary_stops", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_experience_itinerary_stops_tour_experience_itinerary_days_id",
                        column: x => x.itinerary_day_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_itinerary_days",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_tour_experience_itinerary_days_tour_day",
                schema: "tour",
                table: "tour_experience_itinerary_days",
                columns: new[] { "tour_product_id", "day_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tour_experience_itinerary_stops_day_sort",
                schema: "tour",
                table: "tour_experience_itinerary_stops",
                columns: new[] { "itinerary_day_id", "sort_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_experience_itinerary_stops",
                schema: "tour");

            migrationBuilder.DropTable(
                name: "tour_experience_itinerary_days",
                schema: "tour");

            migrationBuilder.DropTable(
                name: "tour_experience_itineraries",
                schema: "tour");
        }
    }
}
