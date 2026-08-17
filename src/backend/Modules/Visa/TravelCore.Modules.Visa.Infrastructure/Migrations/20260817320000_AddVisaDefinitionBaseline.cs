using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Visa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisaDefinitionBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "visa");

            migrationBuilder.CreateTable(
                name: "visa_definitions",
                schema: "visa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visa_definition_translations",
                schema: "visa",
                columns: table => new
                {
                    visa_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_definition_translations", x => new { x.visa_definition_id, x.locale_code });
                    table.ForeignKey(
                        name: "FK_visa_definition_translations_visa_definitions_visa_definiti~",
                        column: x => x.visa_definition_id,
                        principalSchema: "visa",
                        principalTable: "visa_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_requirement_sets",
                schema: "visa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visa_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_requirement_sets", x => x.id);
                    table.ForeignKey(
                        name: "FK_visa_requirement_sets_visa_definitions_visa_definition_id",
                        column: x => x.visa_definition_id,
                        principalSchema: "visa",
                        principalTable: "visa_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_visa_definition_translations_locale_code",
                schema: "visa",
                table: "visa_definition_translations",
                column: "locale_code");

            migrationBuilder.CreateIndex(
                name: "ux_visa_definitions_code",
                schema: "visa",
                table: "visa_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_visa_requirement_sets_visa_definition_id",
                schema: "visa",
                table: "visa_requirement_sets",
                column: "visa_definition_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visa_definition_translations",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_requirement_sets",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_definitions",
                schema: "visa");
        }
    }
}
