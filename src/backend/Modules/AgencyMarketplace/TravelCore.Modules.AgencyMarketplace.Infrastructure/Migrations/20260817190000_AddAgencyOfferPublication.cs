using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyOfferPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "publication_status",
                schema: "agency_marketplace",
                table: "agency_offers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "publication_status",
                schema: "agency_marketplace",
                table: "agency_offers");
        }
    }
}
