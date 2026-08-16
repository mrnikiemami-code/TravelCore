using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Place.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceMediaLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "place_media_links",
                schema: "place",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<short>(type: "smallint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_place_media_links", x => new { x.place_id, x.media_asset_id });
                    table.ForeignKey(
                        name: "FK_place_media_links_places_place_id",
                        column: x => x.place_id,
                        principalSchema: "place",
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_place_media_links_media_asset_id",
                schema: "place",
                table: "place_media_links",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ux_place_media_links_cover",
                schema: "place",
                table: "place_media_links",
                column: "place_id",
                unique: true,
                filter: "role = 0");

            migrationBuilder.CreateIndex(
                name: "ux_place_media_links_gallery_sort",
                schema: "place",
                table: "place_media_links",
                columns: new[] { "place_id", "sort_order" },
                unique: true,
                filter: "role = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "place_media_links",
                schema: "place");
        }
    }
}
