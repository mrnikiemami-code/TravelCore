using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Destination.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DestinationCoverMediaLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "destination_media_links",
                schema: "destination",
                columns: table => new
                {
                    destination_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<short>(type: "smallint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destination_media_links", x => new { x.destination_id, x.media_asset_id });
                    table.ForeignKey(
                        name: "FK_destination_media_links_destinations_destination_id",
                        column: x => x.destination_id,
                        principalSchema: "destination",
                        principalTable: "destinations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_destination_media_links_media_asset_id",
                schema: "destination",
                table: "destination_media_links",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ux_destination_media_links_cover",
                schema: "destination",
                table: "destination_media_links",
                column: "destination_id",
                unique: true,
                filter: "role = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "destination_media_links",
                schema: "destination");
        }
    }
}
