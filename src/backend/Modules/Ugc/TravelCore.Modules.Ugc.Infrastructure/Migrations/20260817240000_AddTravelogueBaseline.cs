using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Ugc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelogueBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "travelogues",
                schema: "ugc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travelogues", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_travelogues_actor_id",
                schema: "ugc",
                table: "travelogues",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_travelogues_locale_code",
                schema: "ugc",
                table: "travelogues",
                column: "locale_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "travelogues",
                schema: "ugc");
        }
    }
}
