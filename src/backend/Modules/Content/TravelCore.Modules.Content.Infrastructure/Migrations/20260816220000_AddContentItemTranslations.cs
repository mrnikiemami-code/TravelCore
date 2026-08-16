using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentItemTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "content_item_translations",
                schema: "content",
                columns: table => new
                {
                    content_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    body = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    excerpt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_item_translations", x => new { x.content_item_id, x.locale_code });
                    table.ForeignKey(
                        name: "FK_content_item_translations_content_items_content_item_id",
                        column: x => x.content_item_id,
                        principalSchema: "content",
                        principalTable: "content_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_item_translations_locale_code",
                schema: "content",
                table: "content_item_translations",
                column: "locale_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_item_translations",
                schema: "content");
        }
    }
}
