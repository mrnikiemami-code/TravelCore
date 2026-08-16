using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentDestinationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_item_destinations",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_item_destinations", x => new { x.content_item_id, x.destination_id });
                    table.ForeignKey(
                        name: "FK_content_item_destinations_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_item_destinations_destination_id",
                schema: "content",
                table: "content_item_destinations",
                column: "destination_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_item_destinations",
                schema: "content");
        }
    }
}
