using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Flight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightBookingCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_tickets_status",
                schema: "flight",
                table: "flight_tickets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues");

            migrationBuilder.AddColumn<Instant>(
                name: "refunded_at",
                schema: "flight",
                table: "flight_tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "voided_at",
                schema: "flight",
                table: "flight_tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "flight_booking_cancellations",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    financial_outcome = table.Column<short>(type: "smallint", nullable: false),
                    penalty_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    completed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_cancellations", x => x.id);
                    table.CheckConstraint("ck_flight_booking_cancellations_financial_outcome", "financial_outcome IN (1, 2)");
                    table.CheckConstraint("ck_flight_booking_cancellations_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_flight_booking_cancellations_flight_bookings_flight_booking~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_booking_cancellation_idempotency",
                schema: "flight",
                columns: table => new
                {
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    flight_booking_cancellation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_supplier_reversal_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_cancellation_idempotency", x => new { x.flight_booking_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_flight_booking_cancellation_idempotency_flight_booking_canc~",
                        column: x => x.flight_booking_cancellation_id,
                        principalSchema: "flight",
                        principalTable: "flight_booking_cancellations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_supplier_reversal_attempts",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_cancellation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    passenger_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    initiated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    succeeded_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_supplier_reversal_attempts", x => x.id);
                    table.CheckConstraint("ck_flight_supplier_reversal_attempts_kind", "kind IN (1, 2, 3)");
                    table.CheckConstraint("ck_flight_supplier_reversal_attempts_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_flight_supplier_reversal_attempts_flight_booking_cancellati~",
                        column: x => x.flight_booking_cancellation_id,
                        principalSchema: "flight",
                        principalTable: "flight_booking_cancellations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_tickets_status",
                schema: "flight",
                table: "flight_tickets",
                sql: "status IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)");

            migrationBuilder.CreateIndex(
                name: "IX_flight_booking_cancellation_idempotency_flight_booking_canc~",
                schema: "flight",
                table: "flight_booking_cancellation_idempotency",
                column: "flight_booking_cancellation_id");

            migrationBuilder.CreateIndex(
                name: "ux_flight_booking_cancellations_flight_booking_id",
                schema: "flight",
                table: "flight_booking_cancellations",
                column: "flight_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_supplier_reversal_attempts_one_unresolved_reservation",
                schema: "flight",
                table: "flight_supplier_reversal_attempts",
                columns: new[] { "flight_booking_cancellation_id", "kind" },
                unique: true,
                filter: "status IN (1, 2) AND ticket_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_flight_supplier_reversal_attempts_one_unresolved_ticket",
                schema: "flight",
                table: "flight_supplier_reversal_attempts",
                columns: new[] { "flight_booking_cancellation_id", "kind", "ticket_id" },
                unique: true,
                filter: "status IN (1, 2) AND ticket_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_booking_cancellation_idempotency",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_supplier_reversal_attempts",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_booking_cancellations",
                schema: "flight");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_tickets_status",
                schema: "flight",
                table: "flight_tickets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues");

            migrationBuilder.DropColumn(
                name: "refunded_at",
                schema: "flight",
                table: "flight_tickets");

            migrationBuilder.DropColumn(
                name: "voided_at",
                schema: "flight",
                table: "flight_tickets");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_tickets_status",
                schema: "flight",
                table: "flight_tickets",
                sql: "status IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4, 5, 6, 7, 8, 9)");
        }
    }
}
