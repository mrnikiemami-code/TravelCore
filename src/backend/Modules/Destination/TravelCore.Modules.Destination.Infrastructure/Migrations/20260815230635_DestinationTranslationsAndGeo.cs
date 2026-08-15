using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Destination.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DestinationTranslationsAndGeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                schema: "destination",
                table: "destinations",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                schema: "destination",
                table: "destinations",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "destination_translations",
                schema: "destination",
                columns: table => new
                {
                    destination_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destination_translations", x => new { x.destination_id, x.locale_code });
                    table.ForeignKey(
                        name: "FK_destination_translations_destinations_destination_id",
                        column: x => x.destination_id,
                        principalSchema: "destination",
                        principalTable: "destinations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_destination_translations_locale_code",
                schema: "destination",
                table: "destination_translations",
                column: "locale_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "destination_translations",
                schema: "destination");

            migrationBuilder.DropColumn(
                name: "latitude",
                schema: "destination",
                table: "destinations");

            migrationBuilder.DropColumn(
                name: "longitude",
                schema: "destination",
                table: "destinations");
        }
    }
}
