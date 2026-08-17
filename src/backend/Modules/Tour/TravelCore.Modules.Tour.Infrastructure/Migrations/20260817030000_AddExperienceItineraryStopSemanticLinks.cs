using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceItineraryStopSemanticLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "destination_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "place_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tour_experience_itinerary_stops_destination_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "ix_tour_experience_itinerary_stops_place_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops",
                column: "place_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tour_experience_itinerary_stops_destination_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops");

            migrationBuilder.DropIndex(
                name: "ix_tour_experience_itinerary_stops_place_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops");

            migrationBuilder.DropColumn(
                name: "destination_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops");

            migrationBuilder.DropColumn(
                name: "place_id",
                schema: "tour",
                table: "tour_experience_itinerary_stops");
        }
    }
}
