using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.TripPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadConsentBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "consent_captured_at",
                schema: "trip_planner",
                table: "leads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<bool>(
                name: "consent_follow_up_contact_allowed",
                schema: "trip_planner",
                table: "leads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "consent_marketing_allowed",
                schema: "trip_planner",
                table: "leads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "consent_preferred_contact_channel",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "consent_privacy_notice_version",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "consent_captured_at",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "consent_follow_up_contact_allowed",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "consent_marketing_allowed",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "consent_preferred_contact_channel",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "consent_privacy_notice_version",
                schema: "trip_planner",
                table: "leads");
        }
    }
}
