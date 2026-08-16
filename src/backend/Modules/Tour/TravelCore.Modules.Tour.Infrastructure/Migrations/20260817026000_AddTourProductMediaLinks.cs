using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourProductMediaLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_product_media_links",
                schema: "tour",
                columns: table => new
                {
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<short>(type: "smallint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_product_media_links", x => new { x.tour_product_id, x.media_asset_id });
                    table.ForeignKey(
                        name: "FK_tour_product_media_links_tour_products_tour_product_id",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_product_media_links_media_asset_id",
                schema: "tour",
                table: "tour_product_media_links",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ux_tour_product_media_links_cover",
                schema: "tour",
                table: "tour_product_media_links",
                column: "tour_product_id",
                unique: true,
                filter: "role = 0");

            migrationBuilder.CreateIndex(
                name: "ux_tour_product_media_links_gallery_sort",
                schema: "tour",
                table: "tour_product_media_links",
                columns: new[] { "tour_product_id", "sort_order" },
                unique: true,
                filter: "role = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_product_media_links",
                schema: "tour");
        }
    }
}
