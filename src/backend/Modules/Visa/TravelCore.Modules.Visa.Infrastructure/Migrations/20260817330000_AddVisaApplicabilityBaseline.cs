using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Visa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisaApplicabilityBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visa_applicabilities",
                schema: "visa",
                columns: table => new
                {
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_geographic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicant_nationality_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    residence_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    applicant_category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_applicabilities", x => x.visa_requirement_set_id);
                    table.ForeignKey(
                        name: "FK_visa_applicabilities_visa_requirement_sets_visa_requirement~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_visa_applicabilities_destination_geographic_id",
                schema: "visa",
                table: "visa_applicabilities",
                column: "destination_geographic_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visa_applicabilities",
                schema: "visa");
        }
    }
}
