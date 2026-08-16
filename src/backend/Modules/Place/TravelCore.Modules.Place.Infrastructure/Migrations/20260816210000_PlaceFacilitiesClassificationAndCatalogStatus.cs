using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Place.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlaceFacilitiesClassificationAndCatalogStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "catalog_status",
                schema: "place",
                table: "places",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "classification_code",
                schema: "place",
                table: "places",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "place_facilities",
                schema: "place",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_place_facilities", x => new { x.place_id, x.code });
                    table.ForeignKey(
                        name: "FK_place_facilities_places_place_id",
                        column: x => x.place_id,
                        principalSchema: "place",
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_places_catalog_status",
                schema: "place",
                table: "places",
                column: "catalog_status");

            migrationBuilder.CreateIndex(
                name: "ix_places_classification_code",
                schema: "place",
                table: "places",
                column: "classification_code");

            migrationBuilder.CreateIndex(
                name: "ix_place_facilities_code",
                schema: "place",
                table: "place_facilities",
                column: "code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "place_facilities",
                schema: "place");

            migrationBuilder.DropIndex(
                name: "ix_places_catalog_status",
                schema: "place",
                table: "places");

            migrationBuilder.DropIndex(
                name: "ix_places_classification_code",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "catalog_status",
                schema: "place",
                table: "places");

            migrationBuilder.DropColumn(
                name: "classification_code",
                schema: "place",
                table: "places");
        }
    }
}
