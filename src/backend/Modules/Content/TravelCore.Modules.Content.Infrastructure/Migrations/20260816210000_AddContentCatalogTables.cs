using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentCatalogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "content_items",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "articles",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articles", x => x.content_item_id);
                    table.ForeignKey(
                        name: "FK_articles_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guides",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guides", x => x.content_item_id);
                    table.ForeignKey(
                        name: "FK_guides_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "landing_pages",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_landing_pages", x => x.content_item_id);
                    table.ForeignKey(
                        name: "FK_landing_pages_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_items_created_at",
                schema: "content",
                table: "content_items",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_content_items_kind",
                schema: "content",
                table: "content_items",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "ux_content_items_code",
                schema: "content",
                table: "content_items",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "articles",
                schema: "content");

            migrationBuilder.DropTable(
                name: "guides",
                schema: "content");

            migrationBuilder.DropTable(
                name: "landing_pages",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_items",
                schema: "content");
        }
    }
}
