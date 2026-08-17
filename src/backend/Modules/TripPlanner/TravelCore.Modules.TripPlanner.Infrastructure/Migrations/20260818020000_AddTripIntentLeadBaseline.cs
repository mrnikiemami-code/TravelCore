using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.TripPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripIntentLeadBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trip_planner");

            migrationBuilder.CreateTable(
                name: "trip_intents",
                schema: "trip_planner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    planning_revision = table.Column<int>(type: "integer", nullable: false),
                    planning_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_intents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                schema: "trip_planner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_trip_intent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    captured_planning_revision = table.Column<int>(type: "integer", nullable: false),
                    captured_planning_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    submitted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.id);
                    table.ForeignKey(
                        name: "FK_leads_trip_intents_source_trip_intent_id",
                        column: x => x.source_trip_intent_id,
                        principalSchema: "trip_planner",
                        principalTable: "trip_intents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_leads_source_trip_intent_id",
                schema: "trip_planner",
                table: "leads",
                column: "source_trip_intent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leads",
                schema: "trip_planner");

            migrationBuilder.DropTable(
                name: "trip_intents",
                schema: "trip_planner");
        }
    }
}
