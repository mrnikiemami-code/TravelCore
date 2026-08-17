using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceGuideAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_experience_guide_assignments",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guide_party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<short>(type: "smallint", nullable: false),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_experience_guide_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_experience_guide_assignments_tour_experience_specializ~",
                        column: x => x.tour_product_id,
                        principalSchema: "tour",
                        principalTable: "tour_experience_specializations",
                        principalColumn: "tour_product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_experience_guide_assignments_guide_party_id",
                schema: "tour",
                table: "tour_experience_guide_assignments",
                column: "guide_party_id");

            migrationBuilder.CreateIndex(
                name: "ux_tour_experience_guide_assignments_tour_party",
                schema: "tour",
                table: "tour_experience_guide_assignments",
                columns: new[] { "tour_product_id", "guide_party_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_experience_guide_assignments",
                schema: "tour");
        }
    }
}
