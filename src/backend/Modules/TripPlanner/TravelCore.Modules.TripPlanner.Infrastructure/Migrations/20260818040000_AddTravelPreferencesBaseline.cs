using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TravelCore.Modules.TripPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelPreferencesBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preference_accommodation",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preference_adult_count",
                schema: "trip_planner",
                table: "trip_intents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preference_approximate_month",
                schema: "trip_planner",
                table: "trip_intents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preference_approximate_season",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preference_approximate_year",
                schema: "trip_planner",
                table: "trip_intents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preference_budget_currency_code",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "preference_budget_max_amount",
                schema: "trip_planner",
                table: "trip_intents",
                type: "numeric(24,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "preference_budget_min_amount",
                schema: "trip_planner",
                table: "trip_intents",
                type: "numeric(24,8)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preference_child_count",
                schema: "trip_planner",
                table: "trip_intents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "preference_exact_end_date",
                schema: "trip_planner",
                table: "trip_intents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "preference_exact_start_date",
                schema: "trip_planner",
                table: "trip_intents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "preference_flexible_earliest_start",
                schema: "trip_planner",
                table: "trip_intents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "preference_flexible_latest_start",
                schema: "trip_planner",
                table: "trip_intents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preference_flexible_max_trip_duration_days",
                schema: "trip_planner",
                table: "trip_intents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preference_infant_count",
                schema: "trip_planner",
                table: "trip_intents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preference_timing_kind",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "preference_transport",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preference_traveler_note",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preference_trip_style",
                schema: "trip_planner",
                table: "trip_intents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_accommodation",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "captured_preference_adult_count",
                schema: "trip_planner",
                table: "leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "captured_preference_approximate_month",
                schema: "trip_planner",
                table: "leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_approximate_season",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "captured_preference_approximate_year",
                schema: "trip_planner",
                table: "leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_budget_currency_code",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "captured_preference_budget_max_amount",
                schema: "trip_planner",
                table: "leads",
                type: "numeric(24,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "captured_preference_budget_min_amount",
                schema: "trip_planner",
                table: "leads",
                type: "numeric(24,8)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "captured_preference_child_count",
                schema: "trip_planner",
                table: "leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "captured_preference_exact_end_date",
                schema: "trip_planner",
                table: "leads",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "captured_preference_exact_start_date",
                schema: "trip_planner",
                table: "leads",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "captured_preference_flexible_earliest_start",
                schema: "trip_planner",
                table: "leads",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "captured_preference_flexible_latest_start",
                schema: "trip_planner",
                table: "leads",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "captured_preference_flexible_max_trip_duration_days",
                schema: "trip_planner",
                table: "leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "captured_preference_infant_count",
                schema: "trip_planner",
                table: "leads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_timing_kind",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_transport",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_traveler_note",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "captured_preference_trip_style",
                schema: "trip_planner",
                table: "leads",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "captured_preference_destination_preferences",
                schema: "trip_planner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    logical_destination_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_undecided = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_captured_preference_destination_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_captured_preference_destination_preferences_leads_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "trip_planner",
                        principalTable: "leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "captured_preference_interest_preferences",
                schema: "trip_planner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interest_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_captured_preference_interest_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_captured_preference_interest_preferences_leads_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "trip_planner",
                        principalTable: "leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preference_destination_preferences",
                schema: "trip_planner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    logical_destination_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_undecided = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preference_destination_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preference_destination_preferences_trip_intents_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "trip_planner",
                        principalTable: "trip_intents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preference_interest_preferences",
                schema: "trip_planner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interest_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preference_interest_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preference_interest_preferences_trip_intents_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "trip_planner",
                        principalTable: "trip_intents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_captured_preference_destination_preferences_OwnerId",
                schema: "trip_planner",
                table: "captured_preference_destination_preferences",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_captured_preference_interest_preferences_OwnerId",
                schema: "trip_planner",
                table: "captured_preference_interest_preferences",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_preference_destination_preferences_OwnerId",
                schema: "trip_planner",
                table: "preference_destination_preferences",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_preference_interest_preferences_OwnerId",
                schema: "trip_planner",
                table: "preference_interest_preferences",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "captured_preference_destination_preferences",
                schema: "trip_planner");

            migrationBuilder.DropTable(
                name: "captured_preference_interest_preferences",
                schema: "trip_planner");

            migrationBuilder.DropTable(
                name: "preference_destination_preferences",
                schema: "trip_planner");

            migrationBuilder.DropTable(
                name: "preference_interest_preferences",
                schema: "trip_planner");

            migrationBuilder.DropColumn(
                name: "preference_accommodation",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_adult_count",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_approximate_month",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_approximate_season",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_approximate_year",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_budget_currency_code",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_budget_max_amount",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_budget_min_amount",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_child_count",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_exact_end_date",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_exact_start_date",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_flexible_earliest_start",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_flexible_latest_start",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_flexible_max_trip_duration_days",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_infant_count",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_timing_kind",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_transport",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_traveler_note",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "preference_trip_style",
                schema: "trip_planner",
                table: "trip_intents");

            migrationBuilder.DropColumn(
                name: "captured_preference_accommodation",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_adult_count",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_approximate_month",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_approximate_season",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_approximate_year",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_budget_currency_code",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_budget_max_amount",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_budget_min_amount",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_child_count",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_exact_end_date",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_exact_start_date",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_flexible_earliest_start",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_flexible_latest_start",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_flexible_max_trip_duration_days",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_infant_count",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_timing_kind",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_transport",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_traveler_note",
                schema: "trip_planner",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "captured_preference_trip_style",
                schema: "trip_planner",
                table: "leads");
        }
    }
}
