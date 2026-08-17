using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourDepartureScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_departures",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_departures", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_departures_tour_products_tour_product_id",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_departures_created_at",
                schema: "tour",
                table: "tour_departures",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tour_departures_tour_product_id",
                schema: "tour",
                table: "tour_departures",
                column: "tour_product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_departures",
                schema: "tour");
        }
    }
}
