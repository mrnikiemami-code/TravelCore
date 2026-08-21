using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Pricing.Infrastructure.Migrations;

[DbContext(typeof(PricingDbContext))]
[Migration("20260822020000_P38QuoteCommercialContextAgencyOffer")]
public partial class P38QuoteCommercialContextAgencyOffer : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "commercial_context_agency_offer_id",
            schema: "pricing",
            table: "quotes",
            type: "uuid",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "commercial_context_agency_offer_id",
            schema: "pricing",
            table: "quotes");
    }
}
