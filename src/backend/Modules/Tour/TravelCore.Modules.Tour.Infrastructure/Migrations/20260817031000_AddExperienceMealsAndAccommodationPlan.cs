using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceMealsAndAccommodationPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_experience_accommodation_plan",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_accommodation_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_experience_accommodation_plan_tour_experience_specializations_tour_product_id",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_specializations",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_experience_day_meals",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itinerary_day_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meal_type = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_day_meals", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_experience_day_meals_tour_experience_itinerary_days_itinerary_day_id",
                        column: x => x.itinerary_day_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_itinerary_days",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_experience_accommodation_plan_place_id",
                schema: "tour",
                table: "tour_experience_accommodation_plan",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "ux_tour_experience_accommodation_plan_tour_sort",
                schema: "tour",
                table: "tour_experience_accommodation_plan",
                columns: new[] { "tour_product_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tour_experience_day_meals_day_type",
                schema: "tour",
                table: "tour_experience_day_meals",
                columns: new[] { "itinerary_day_id", "meal_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_experience_accommodation_plan",
                schema: "tour");

            migrationBuilder.DropTable(
                name: "tour_experience_day_meals",
                schema: "tour");
        }
    }
}
