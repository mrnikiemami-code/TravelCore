using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceOperationalAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "difficulty",
                schema: "tour",
                table: "tour_experience_specializations",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tour_experience_eligibility_requirements",
                schema: "tour",
                columns: table => new
                {
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_eligibility_requirements", x => new { x.tour_product_id, x.code });
                    table.ForeignKey(
                        name: "FK_tour_experience_eligibility_requirements_tour_experience_sp~",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_specializations",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_experience_equipment",
                schema: "tour",
                columns: table => new
                {
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_equipment", x => new { x.tour_product_id, x.code });
                    table.ForeignKey(
                        name: "FK_tour_experience_equipment_tour_experience_specializations_t~",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_specializations",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_experience_local_transport",
                schema: "tour",
                columns: table => new
                {
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_local_transport", x => new { x.tour_product_id, x.code });
                    table.ForeignKey(
                        name: "FK_tour_experience_local_transport_tour_experience_specializat~",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_specializations",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_experience_eligibility_requirements",
                schema: "tour");

            migrationBuilder.DropTable(
                name: "tour_experience_equipment",
                schema: "tour");

            migrationBuilder.DropTable(
                name: "tour_experience_local_transport",
                schema: "tour");

            migrationBuilder.DropColumn(
                name: "difficulty",
                schema: "tour",
                table: "tour_experience_specializations");
        }
    }
}
