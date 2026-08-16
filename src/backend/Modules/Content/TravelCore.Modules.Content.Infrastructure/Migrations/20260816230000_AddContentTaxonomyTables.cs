using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTaxonomyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "content_categories",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_tags",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_item_categories",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_item_categories", x => new { x.content_item_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_content_item_categories_content_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "content",
                        principalTable: "content_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_content_item_categories_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_item_tags",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_item_tags", x => new { x.content_item_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_content_item_tags_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_item_tags_content_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "content",
                        principalTable: "content_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_content_categories_code",
                schema: "content",
                table: "content_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_item_categories_category_id",
                schema: "content",
                table: "content_item_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_item_tags_tag_id",
                schema: "content",
                table: "content_item_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ux_content_tags_code",
                schema: "content",
                table: "content_tags",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_item_categories",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_item_tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_categories",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_tags",
                schema: "content");
        }
    }
}
