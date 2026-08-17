using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAgencyMarketplaceScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agency_marketplace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema drop is intentionally omitted: other Agency Marketplace objects may appear in later migrations.
        }
    }
}
