using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelBookingPaymentIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "cancelled_at",
                schema: "hotel_booking",
                table: "hotel_bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "version",
                schema: "hotel_booking",
                table: "hotel_bookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "hotel_booking_payment_compensation_evidence",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<short>(type: "smallint", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_payment_compensation_evidence", x => x.id);
                    table.CheckConstraint("ck_hotel_booking_payment_compensation_reason", "reason IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)");
                    table.ForeignKey(
                        name: "FK_hotel_booking_payment_compensation_evidence_hotel_bookings_~",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hotel_booking_payment_evidence",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    verified_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_payment_evidence", x => x.hotel_booking_id);
                    table.ForeignKey(
                        name: "FK_hotel_booking_payment_evidence_hotel_bookings_hotel_booking~",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hotel_booking_refund_invariant_issues",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_refund_invariant_issues", x => x.id);
                    table.CheckConstraint("ck_hotel_booking_refund_invariant_kind", "kind IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_hotel_booking_refund_invariant_issues_hotel_bookings_hotel_~",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "hotel_booking",
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

            migrationBuilder.CreateTable(
                name: "payment_success_inbox",
                schema: "hotel_booking",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_success_inbox", x => x.payment_id);
                });

            migrationBuilder.CreateTable(
                name: "refund_success_inbox",
                schema: "hotel_booking",
                columns: table => new
                {
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_success_inbox", x => x.refund_id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_payment_compensation_evidence_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_booking_payment_compensation_evidence",
                column: "hotel_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_payment_evidence_payment_id",
                schema: "hotel_booking",
                table: "hotel_booking_payment_evidence",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hotel_booking_refund_invariant_issues_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_booking_refund_invariant_issues",
                column: "hotel_booking_id");

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_refund_invariant_issues_refund_id",
                schema: "hotel_booking",
                table: "hotel_booking_refund_invariant_issues",
                column: "refund_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hotel_booking_outbox_messages_processed_at",
                schema: "hotel_booking",
                table: "outbox_messages",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_booking_payment_compensation_evidence",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_booking_payment_evidence",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_booking_refund_invariant_issues",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "payment_success_inbox",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "refund_success_inbox",
                schema: "hotel_booking");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                schema: "hotel_booking",
                table: "hotel_bookings");

            migrationBuilder.DropColumn(
                name: "version",
                schema: "hotel_booking",
                table: "hotel_bookings");
        }
    }
}
