using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Migrations;

[DbContext(typeof(AgencyMarketplaceDbContext))]
[Migration("20260822040000_P38AgencyOfferGovernanceAudit")]
public partial class P38AgencyOfferGovernanceAudit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "agency_offer_governance_events",
            schema: "agency_marketplace",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                agency_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                kind = table.Column<short>(type: "smallint", nullable: false),
                actor_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                actor_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                from_publication_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                to_publication_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                policy_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                policy_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_agency_offer_governance_events", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_agency_offer_governance_events_offer_occurred",
            schema: "agency_marketplace",
            table: "agency_offer_governance_events",
            columns: new[] { "offer_id", "occurred_at" });

        migrationBuilder.CreateIndex(
            name: "ix_agency_offer_governance_events_agency_profile",
            schema: "agency_marketplace",
            table: "agency_offer_governance_events",
            column: "agency_profile_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "agency_offer_governance_events",
            schema: "agency_marketplace");
    }
}
