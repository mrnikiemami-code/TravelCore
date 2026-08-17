using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyOfferCapacityBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "referenced_tour_departure_id",
                schema: "agency_marketplace",
                table: "agency_offers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "sales_open",
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
                name: "referenced_tour_departure_id",
                schema: "agency_marketplace",
                table: "agency_offers");

            migrationBuilder.DropColumn(
                name: "sales_open",
                schema: "agency_marketplace",
                table: "agency_offers");
        }
    }
}
