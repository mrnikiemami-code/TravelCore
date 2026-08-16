using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Place.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceCatalogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "place");

            migrationBuilder.CreateTable(
                name: "places",
                schema: "place",
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
                    table.PrimaryKey("PK_places", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "attractions",
                schema: "place",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attractions", x => x.place_id);
                    table.ForeignKey(
                        name: "FK_attractions_places_place_id",
                        column: x => x.place_id,
                        principalSchema: "place",
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotels",
                schema: "place",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    star_rating = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotels", x => x.place_id);
                    table.ForeignKey(
                        name: "FK_hotels_places_place_id",
                        column: x => x.place_id,
                        principalSchema: "place",
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                schema: "place",
                columns: table => new
                {
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuisine_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurants", x => x.place_id);
                    table.ForeignKey(
                        name: "FK_restaurants_places_place_id",
                        column: x => x.place_id,
                        principalSchema: "place",
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_places_created_at",
                schema: "place",
                table: "places",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_places_kind",
                schema: "place",
                table: "places",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "ux_places_code",
                schema: "place",
                table: "places",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attractions",
                schema: "place");

            migrationBuilder.DropTable(
                name: "hotels",
                schema: "place");

            migrationBuilder.DropTable(
                name: "restaurants",
                schema: "place");

            migrationBuilder.DropTable(
                name: "places",
                schema: "place");
        }
    }
}
