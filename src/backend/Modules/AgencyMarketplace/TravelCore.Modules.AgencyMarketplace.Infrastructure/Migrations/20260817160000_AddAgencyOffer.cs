using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agency_offers",
                schema: "agency_marketplace",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_override = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    highlight = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    commercial_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    visibility = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_offers", x => x.id);
                    table.ForeignKey(
                        name: "FK_agency_offers_agency_profiles_agency_profile_id",
                        column: x => x.agency_profile_id,
                        principalSchema: "agency_marketplace",
                        principalTable: "agency_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agency_offers_tour_product_id",
                schema: "agency_marketplace",
                table: "agency_offers",
                column: "tour_product_id");

            migrationBuilder.CreateIndex(
                name: "ux_agency_offers_profile_tour_product",
                schema: "agency_marketplace",
                table: "agency_offers",
                columns: new[] { "agency_profile_id", "tour_product_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agency_offers",
                schema: "agency_marketplace");
        }
    }
}
