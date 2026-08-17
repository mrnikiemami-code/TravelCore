using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyOfferCommercialBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "exclusive_listing",
                schema: "agency_marketplace",
                table: "agency_offers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "requires_manual_confirmation",
                schema: "agency_marketplace",
                table: "agency_offers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exclusive_listing",
                schema: "agency_marketplace",
                table: "agency_offers");

            migrationBuilder.DropColumn(
                name: "requires_manual_confirmation",
                schema: "agency_marketplace",
                table: "agency_offers");
        }
    }
}
