using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.TripPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripPlannerIdentityContactBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "actor_reference_id",
                schema: "trip_planner",
                table: "trip_intents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "draft_access_token",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "actor_reference_id",
                schema: "trip_planner",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_display_name",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_email",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_normalized_email",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_phone",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_trip_intents_draft_access_token",
                schema: "trip_planner",
                table: "trip_intents",
                column: "draft_access_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_trip_intents_draft_access_token",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "actor_reference_id",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "draft_access_token",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "actor_reference_id",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "contact_display_name",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "contact_email",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "contact_normalized_email",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "contact_phone",
                schema: "trip_planner",
                table: "leads");
        }
    }
}
