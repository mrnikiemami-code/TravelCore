using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Destination.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDestinationPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "destination");

            migrationBuilder.CreateTable(
                name: "destinations",
                schema: "destination",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    iso_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destinations", x => x.id);
                    table.ForeignKey(
                        name: "FK_destinations_destinations_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "destination",
                        principalTable: "destinations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_destinations_iso_country_code",
                schema: "destination",
                table: "destinations",
                column: "iso_country_code");

            migrationBuilder.CreateIndex(
                name: "ix_destinations_kind",
                schema: "destination",
                table: "destinations",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "ix_destinations_parent_id",
                schema: "destination",
                table: "destinations",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ux_destinations_code",
                schema: "destination",
                table: "destinations",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "destinations",
                schema: "destination");
        }
    }
}
