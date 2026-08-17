using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Visa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisaProcessingValidityBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "effective_from",
                schema: "visa",
                table: "visa_requirement_sets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "effective_to",
                schema: "visa",
                table: "visa_requirement_sets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "visa_allowed_stays",
                schema: "visa",
                columns: table => new
                {
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_allowed_stays", x => x.visa_requirement_set_id);
                    table.ForeignKey(
                        name: "FK_visa_allowed_stays_visa_requirement_sets_visa_requirement_s~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_entry_policies",
                schema: "visa",
                columns: table => new
                {
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_entry_policies", x => x.visa_requirement_set_id);
                    table.ForeignKey(
                        name: "FK_visa_entry_policies_visa_requirement_sets_visa_requirement_~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_processing_times",
                schema: "visa",
                columns: table => new
                {
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_value = table.Column<int>(type: "integer", nullable: false),
                    max_value = table.Column<int>(type: "integer", nullable: true),
                    unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_processing_times", x => x.visa_requirement_set_id);
                    table.ForeignKey(
                        name: "FK_visa_processing_times_visa_requirement_sets_visa_requiremen~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visa_validities",
                schema: "visa",
                columns: table => new
                {
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_validities", x => x.visa_requirement_set_id);
                    table.ForeignKey(
                        name: "FK_visa_validities_visa_requirement_sets_visa_requirement_set_~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visa_allowed_stays",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_entry_policies",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_processing_times",
                schema: "visa");

            migrationBuilder.DropTable(
                name: "visa_validities",
                schema: "visa");

            migrationBuilder.DropColumn(
                name: "effective_from",
                schema: "visa",
                table: "visa_requirement_sets");

            migrationBuilder.DropColumn(
                name: "effective_to",
                schema: "visa",
                table: "visa_requirement_sets");
        }
    }
}
