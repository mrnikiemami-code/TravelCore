using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Visa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisaRequirementFactsBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visa_eligibility_requirements",
                schema: "visa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    requirement_level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_eligibility_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_visa_eligibility_requirements_visa_requirement_sets_visa_re~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_required_documents",
                schema: "visa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    requirement_level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_required_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_visa_required_documents_visa_requirement_sets_visa_requirem~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_eligibility_requirement_translations",
                schema: "visa",
                columns: table => new
                {
                    eligibility_requirement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_eligibility_requirement_translations", x => new { x.eligibility_requirement_id, x.locale_code });
                    table.ForeignKey(
                        name: "FK_visa_eligibility_requirement_translations_visa_eligibility_~",
                        column: x => x.eligibility_requirement_id,
                        principalSchema: "visa",
                        principalTable: "visa_eligibility_requirements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_required_document_translations",
                schema: "visa",
                columns: table => new
                {
                    required_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_required_document_translations", x => new { x.required_document_id, x.locale_code });
                    table.ForeignKey(
                        name: "FK_visa_required_document_translations_visa_required_documents~",
                        column: x => x.required_document_id,
                        principalSchema: "visa",
                        principalTable: "visa_required_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_visa_eligibility_requirements_set_code",
                schema: "visa",
                table: "visa_eligibility_requirements",
                columns: new[] { "visa_requirement_set_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_visa_required_documents_set_code",
                schema: "visa",
                table: "visa_required_documents",
                columns: new[] { "visa_requirement_set_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visa_eligibility_requirement_translations",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_required_document_translations",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_eligibility_requirements",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_required_documents",
                schema: "visa");
        }
    }
}
