using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.CommercialFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class P39CommercialFinancePersistenceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "commercial_obligations",
                schema: "commercial_finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_offer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lifecycle_state = table.Column<short>(type: "smallint", nullable: false),
                    source_event_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    amount_snapshot_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    amount_snapshot_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    evidence_snapshot_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    state_changed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commercial_obligations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commission_agreements",
                schema: "commercial_finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    market_policy = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    effective_from = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commission_agreements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settlement_periods",
                schema: "commercial_finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    period_start = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_periods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_consumption_records",
                schema: "commercial_finance",
                columns: table => new
                {
                    source_kind = table.Column<short>(type: "smallint", nullable: false),
                    source_event_key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    obligation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_consumption_records", x => new { x.source_kind, x.source_event_key });
                    table.ForeignKey(
                        name: "FK_event_consumption_records_commercial_obligations_obligation~",
                        column: x => x.obligation_id,
                        principalSchema: "commercial_finance",
                        principalTable: "commercial_obligations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agency_offer_commission_overrides",
                schema: "commercial_finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commission_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_offer_commission_overrides", x => x.id);
                    table.ForeignKey(
                        name: "FK_agency_offer_commission_overrides_commission_agreements_com~",
                        column: x => x.commission_agreement_id,
                        principalSchema: "commercial_finance",
                        principalTable: "commission_agreements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "settlement_records",
                schema: "commercial_finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    approval_required = table.Column<bool>(type: "boolean", nullable: false),
                    approved_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_settlement_records_settlement_periods_settlement_period_id",
                        column: x => x.settlement_period_id,
                        principalSchema: "commercial_finance",
                        principalTable: "settlement_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payout_instructions",
                schema: "commercial_finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    payout_amount_snapshot_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    payout_amount_snapshot_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    approval_required = table.Column<bool>(type: "boolean", nullable: false),
                    approved_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payout_instructions", x => x.id);
                    table.ForeignKey(
                        name: "FK_payout_instructions_settlement_records_settlement_record_id",
                        column: x => x.settlement_record_id,
                        principalSchema: "commercial_finance",
                        principalTable: "settlement_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agency_offer_commission_overrides_commission_agreement_id",
                schema: "commercial_finance",
                table: "agency_offer_commission_overrides",
                column: "commission_agreement_id");

            migrationBuilder.CreateIndex(
                name: "ux_agency_offer_commission_overrides_agency_offer_id",
                schema: "commercial_finance",
                table: "agency_offer_commission_overrides",
                column: "agency_offer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commercial_obligations_agency_profile_lifecycle",
                schema: "commercial_finance",
                table: "commercial_obligations",
                columns: new[] { "agency_profile_id", "lifecycle_state" });

            migrationBuilder.CreateIndex(
                name: "ux_commercial_obligations_source_event_key",
                schema: "commercial_finance",
                table: "commercial_obligations",
                column: "source_event_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commission_agreements_agency_profile_id",
                schema: "commercial_finance",
                table: "commission_agreements",
                column: "agency_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_consumption_records_obligation_id",
                schema: "commercial_finance",
                table: "event_consumption_records",
                column: "obligation_id");

            migrationBuilder.CreateIndex(
                name: "IX_payout_instructions_settlement_record_id",
                schema: "commercial_finance",
                table: "payout_instructions",
                column: "settlement_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_settlement_periods_agency_profile_status",
                schema: "commercial_finance",
                table: "settlement_periods",
                columns: new[] { "agency_profile_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_settlement_records_settlement_period_id",
                schema: "commercial_finance",
                table: "settlement_records",
                column: "settlement_period_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agency_offer_commission_overrides",
                schema: "commercial_finance");

            migrationBuilder.DropTable(
                name: "event_consumption_records",
                schema: "commercial_finance");

            migrationBuilder.DropTable(
                name: "payout_instructions",
                schema: "commercial_finance");

            migrationBuilder.DropTable(
                name: "commission_agreements",
                schema: "commercial_finance");

            migrationBuilder.DropTable(
                name: "commercial_obligations",
                schema: "commercial_finance");

            migrationBuilder.DropTable(
                name: "settlement_records",
                schema: "commercial_finance");

            migrationBuilder.DropTable(
                name: "settlement_periods",
                schema: "commercial_finance");
        }
    }
}
