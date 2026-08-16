using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TravelCore.Modules.Content.Infrastructure;

#nullable disable

namespace TravelCore.Modules.Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ContentDbContext))]
    [Migration("20260816260000_AddContentItemTranslationSlugs")]
    public partial class AddContentItemTranslationSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "content",
                table: "content_item_translations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_content_item_translations_locale_slug",
                schema: "content",
                table: "content_item_translations",
                columns: new[] { "locale_code", "slug" },
                unique: true,
                filter: "slug IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_content_item_translations_locale_slug",
                schema: "content",
                table: "content_item_translations");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "content",
                table: "content_item_translations");
        }
    }
}
