using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourProductSemanticLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "classification_code",
                schema: "tour",
                table: "tour_products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "origin_destination_id",
                schema: "tour",
                table: "tour_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tour_product_destinations",
                schema: "tour",
                columns: table => new
                {
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_product_destinations", x => new { x.tour_product_id, x.destination_id });
                    table.ForeignKey(
                        name: "FK_tour_product_destinations_tour_products_tour_product_id",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_products_classification_code",
                schema: "tour",
                table: "tour_products",
                column: "classification_code");

            migrationBuilder.CreateIndex(
                name: "ix_tour_products_origin_destination_id",
                schema: "tour",
                table: "tour_products",
                column: "origin_destination_id");

            migrationBuilder.CreateIndex(
                name: "ix_tour_product_destinations_destination_id",
                schema: "tour",
                table: "tour_product_destinations",
                column: "destination_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_product_destinations",
                schema: "tour");

            migrationBuilder.DropIndex(
                name: "ix_tour_products_classification_code",
                schema: "tour",
                table: "tour_products");

            migrationBuilder.DropIndex(
                name: "ix_tour_products_origin_destination_id",
                schema: "tour",
                table: "tour_products");

            migrationBuilder.DropColumn(
                name: "classification_code",
                schema: "tour",
                table: "tour_products");

            migrationBuilder.DropColumn(
                name: "origin_destination_id",
                schema: "tour",
                table: "tour_products");
        }
    }
}
