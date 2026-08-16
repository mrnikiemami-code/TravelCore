using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentBlocksTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "content_blocks",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    heading_level = table.Column<short>(type: "smallint", nullable: true),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContentItemId2 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_blocks_content_items_ContentItemId2",
                        column: x => x.ContentItemId2,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_content_blocks_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_block_faq_items",
                schema: "content",
                columns: table => new
                {
                    block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    answer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_block_faq_items", x => new { x.block_id, x.sort_order });
                    table.ForeignKey(
                        name: "FK_content_block_faq_items_content_blocks_block_id",
                        column: x => x.block_id,
                        principalSchema: "content",
                        principalTable: "content_blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_block_gallery_items",
                schema: "content",
                columns: table => new
                {
                    block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_block_gallery_items", x => new { x.block_id, x.media_asset_id });
                    table.ForeignKey(
                        name: "FK_content_block_gallery_items_content_blocks_block_id",
                        column: x => x.block_id,
                        principalSchema: "content",
                        principalTable: "content_blocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_content_block_gallery_items_sort",
                schema: "content",
                table: "content_block_gallery_items",
                columns: new[] { "block_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_blocks_ContentItemId2",
                schema: "content",
                table: "content_blocks",
                column: "ContentItemId2");

            migrationBuilder.CreateIndex(
                name: "ix_content_blocks_item_sort",
                schema: "content",
                table: "content_blocks",
                columns: new[] { "content_item_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_content_blocks_kind",
                schema: "content",
                table: "content_blocks",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "ix_content_blocks_media_asset_id",
                schema: "content",
                table: "content_blocks",
                column: "media_asset_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_block_faq_items",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_block_gallery_items",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_blocks",
                schema: "content");
        }
    }
}
