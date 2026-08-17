using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agency_marketplace");

            migrationBuilder.CreateTable(
                name: "agency_profiles",
                schema: "agency_marketplace",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    logo_media_asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    public_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    public_phone = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    website_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    public_listing_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_profiles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_agency_profiles_party_id",
                schema: "agency_marketplace",
                table: "agency_profiles",
                column: "party_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agency_profiles",
                schema: "agency_marketplace");
        }
    }
}
