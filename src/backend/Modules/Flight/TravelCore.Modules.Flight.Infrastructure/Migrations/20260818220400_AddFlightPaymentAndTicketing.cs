using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Flight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightPaymentAndTicketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues");

            migrationBuilder.AddColumn<Instant>(
                name: "cancelled_at",
                schema: "flight",
                table: "flight_bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "confirmed_at",
                schema: "flight",
                table: "flight_bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "status",
                schema: "flight",
                table: "flight_bookings",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            migrationBuilder.AddColumn<long>(
                name: "version",
                schema: "flight",
                table: "flight_bookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "flight_booking_payment_compensation_evidence",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<short>(type: "smallint", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_payment_compensation_evidence", x => x.id);
                    table.CheckConstraint("ck_flight_booking_payment_compensation_reason", "reason IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_flight_booking_payment_compensation_evidence_flight_booking~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "flight_booking_payment_evidence",
                schema: "flight",
                columns: table => new
                {
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    verified_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_payment_evidence", x => x.flight_booking_id);
                    table.ForeignKey(
                        name: "FK_flight_booking_payment_evidence_flight_bookings_flight_book~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "flight_payment_success_inbox",
                schema: "flight",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_payment_success_inbox", x => x.payment_id);
                });

            migrationBuilder.CreateTable(
                name: "flight_refund_success_inbox",
                schema: "flight",
                columns: table => new
                {
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_refund_success_inbox", x => x.refund_id);
                });

            migrationBuilder.CreateTable(
                name: "flight_ticketing_attempts",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    initiated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    succeeded_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_ticketing_attempts", x => x.id);
                    table.CheckConstraint("ck_flight_ticketing_attempts_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_flight_ticketing_attempts_flight_bookings_flight_booking_id",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_ticketing_idempotency",
                schema: "flight",
                columns: table => new
                {
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    flight_ticketing_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_ticketing_idempotency", x => new { x.flight_booking_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_flight_ticketing_idempotency_flight_bookings_flight_booking~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_tickets",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_passenger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    source_ticket_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    issued_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_tickets", x => x.id);
                    table.CheckConstraint("ck_flight_tickets_status", "status IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_flight_tickets_flight_bookings_flight_booking_id",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    message_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4, 5, 6, 7, 8, 9)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_bookings_status",
                schema: "flight",
                table: "flight_bookings",
                sql: "status IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_bookings_version_nonnegative",
                schema: "flight",
                table: "flight_bookings",
                sql: "version >= 0");

            migrationBuilder.CreateIndex(
                name: "ux_flight_booking_payment_compensation_evidence_flight_booking_id",
                schema: "flight",
                table: "flight_booking_payment_compensation_evidence",
                column: "flight_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_booking_payment_evidence_payment_id",
                schema: "flight",
                table: "flight_booking_payment_evidence",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_ticketing_attempts_one_unresolved",
                schema: "flight",
                table: "flight_ticketing_attempts",
                column: "flight_booking_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ux_flight_tickets_booking_passenger",
                schema: "flight",
                table: "flight_tickets",
                columns: new[] { "flight_booking_id", "flight_passenger_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_tickets_source_ticket_number",
                schema: "flight",
                table: "flight_tickets",
                column: "source_ticket_number",
                unique: true,
                filter: "source_ticket_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_flight_outbox_messages_processed_at",
                schema: "flight",
                table: "outbox_messages",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_booking_payment_compensation_evidence",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_booking_payment_evidence",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_payment_success_inbox",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_refund_success_inbox",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_ticketing_attempts",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_ticketing_idempotency",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_tickets",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "flight");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_bookings_status",
                schema: "flight",
                table: "flight_bookings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_flight_bookings_version_nonnegative",
                schema: "flight",
                table: "flight_bookings");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                schema: "flight",
                table: "flight_bookings");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                schema: "flight",
                table: "flight_bookings");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "flight",
                table: "flight_bookings");

            migrationBuilder.DropColumn(
                name: "version",
                schema: "flight",
                table: "flight_bookings");

            migrationBuilder.AddCheckConstraint(
                name: "ck_flight_reconciliation_issues_kind",
                schema: "flight",
                table: "flight_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4, 5, 6, 7)");
        }
    }
}
