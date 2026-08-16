using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Place.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceTranslationsDestinationLinkAndGeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_administrative_area",
                schema: "place",
                table: "places",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_country_code",
                schema: "place",
                table: "places",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line1",
                schema: "place",
                table: "places",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line2",
                schema: "place",
                table: "places",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_locality",
                schema: "place",
                table: "places",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_postal_code",
                schema: "place",
                table: "places",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "destination_id",
                schema: "place",
                table: "places",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                schema: "place",
                table: "places",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                schema: "place",
                table: "places",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "place_translations",
                schema: "place",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_place_translations", x => new { x.place_id, x.locale_code });
                    table.ForeignKey(
                        name: "FK_place_translations_places_place_id",
                        column: x => x.place_id,
                        principalSchema: "place",
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_places_destination_id",
                schema: "place",
                table: "places",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "ix_place_translations_locale_code",
                schema: "place",
                table: "place_translations",
                column: "locale_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "place_translations",
                schema: "place");

            migrationBuilder.DropIndex(
                name: "ix_places_destination_id",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "address_administrative_area",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "address_country_code",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "address_line1",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "address_line2",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "address_locality",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "address_postal_code",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "destination_id",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "latitude",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "longitude",
                schema: "place",
                table: "places");
        }
    }
}
