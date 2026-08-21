using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations;

[DbContext(typeof(AgencyMarketplaceDbContext))]
[Migration("20260822010000_P38AgencyOfferPersistenceFoundation")]
public partial class P38AgencyOfferPersistenceFoundation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<short>(
            name: "sales_channel",
            schema: "agency_marketplace",
            table: "agency_offers",
            type: "smallint",
            nullable: false,
            defaultValue: (short)1);

        migrationBuilder.AddColumn<short>(
            name: "departure_scope_mode",
            schema: "agency_marketplace",
            table: "agency_offers",
            type: "smallint",
            nullable: false,
            defaultValue: (short)1);

        migrationBuilder.AddColumn<Guid[]>(
            name: "departure_scope_ids",
            schema: "agency_marketplace",
            table: "agency_offers",
            type: "uuid[]",
            nullable: false,
            defaultValueSql: "'{}'::uuid[]");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "created_at",
            schema: "agency_marketplace",
            table: "agency_offers",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "TIMESTAMPTZ '1970-01-01 00:00:00+00'");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "updated_at",
            schema: "agency_marketplace",
            table: "agency_offers",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "TIMESTAMPTZ '1970-01-01 00:00:00+00'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "sales_channel",
            schema: "agency_marketplace",
            table: "agency_offers");

        migrationBuilder.DropColumn(
            name: "departure_scope_mode",
            schema: "agency_marketplace",
            table: "agency_offers");

        migrationBuilder.DropColumn(
            name: "departure_scope_ids",
            schema: "agency_marketplace",
            table: "agency_offers");

        migrationBuilder.DropColumn(
            name: "created_at",
            schema: "agency_marketplace",
            table: "agency_offers");

        migrationBuilder.DropColumn(
            name: "updated_at",
            schema: "agency_marketplace",
            table: "agency_offers");
    }
}
