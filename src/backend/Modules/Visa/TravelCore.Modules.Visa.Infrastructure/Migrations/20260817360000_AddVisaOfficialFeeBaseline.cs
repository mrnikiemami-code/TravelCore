using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Visa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisaOfficialFeeBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visa_official_fees",
                schema: "visa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visa_requirement_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visa_official_fees", x => x.id);
                    table.ForeignKey(
                        name: "FK_visa_official_fees_visa_requirement_sets_visa_requirement_s~",
                        column: x => x.visa_requirement_set_id,
                        principalSchema: "visa",
                        principalTable: "visa_requirement_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_visa_official_fees_set_kind",
                schema: "visa",
                table: "visa_official_fees",
                columns: new[] { "visa_requirement_set_id", "kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visa_official_fees",
                schema: "visa");
        }
    }
}
