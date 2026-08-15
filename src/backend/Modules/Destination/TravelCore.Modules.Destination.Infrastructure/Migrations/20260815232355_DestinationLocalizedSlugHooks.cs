using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Destination.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DestinationLocalizedSlugHooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "destination",
                table: "destination_translations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_destination_translations_locale_slug",
                schema: "destination",
                table: "destination_translations",
                columns: new[] { "locale_code", "slug" },
                unique: true,
                filter: "slug IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_destination_translations_locale_slug",
                schema: "destination",
                table: "destination_translations");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "destination",
                table: "destination_translations");
        }
    }
}
