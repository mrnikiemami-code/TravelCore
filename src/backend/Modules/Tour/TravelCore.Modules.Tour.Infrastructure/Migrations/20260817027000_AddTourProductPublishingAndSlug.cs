using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourProductPublishingAndSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "catalog_status",
                schema: "tour",
                table: "tour_products",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "tour",
                table: "tour_product_translations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tour_products_catalog_status",
                schema: "tour",
                table: "tour_products",
                column: "catalog_status");

            migrationBuilder.CreateIndex(
                name: "ux_tour_product_translations_locale_slug",
                schema: "tour",
                table: "tour_product_translations",
                columns: new[] { "locale_code", "slug" },
                unique: true,
                filter: "slug IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tour_products_catalog_status",
                schema: "tour",
                table: "tour_products");

            migrationBuilder.DropIndex(
                name: "ux_tour_product_translations_locale_slug",
                schema: "tour",
                table: "tour_product_translations");

            migrationBuilder.DropColumn(
                name: "catalog_status",
                schema: "tour",
                table: "tour_products");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "tour",
                table: "tour_product_translations");
        }
    }
}
