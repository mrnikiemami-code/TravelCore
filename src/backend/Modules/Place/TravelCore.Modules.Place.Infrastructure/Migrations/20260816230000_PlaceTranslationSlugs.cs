using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Place.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceTranslationSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "place",
                table: "place_translations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_place_translations_locale_slug",
                schema: "place",
                table: "place_translations",
                columns: new[] { "locale_code", "slug" },
                unique: true,
                filter: "slug IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_place_translations_locale_slug",
                schema: "place",
                table: "place_translations");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "place",
                table: "place_translations");
        }
    }
}
