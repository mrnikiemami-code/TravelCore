using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCompensationOutboxAndRefundInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "booking_refund_invariant_issues",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_refund_invariant_issues", x => x.id);
                    table.CheckConstraint("ck_booking_refund_invariant_issues_kind", "kind IN (1)");
                    table.ForeignKey(
                        name: "FK_booking_refund_invariant_issues_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "booking",
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
                name: "refund_success_inbox",
                schema: "booking",
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
                name: "IX_booking_refund_invariant_issues_booking_id",
                schema: "booking",
                table: "booking_refund_invariant_issues",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ux_booking_refund_invariant_issues_refund_id",
                schema: "booking",
                table: "booking_refund_invariant_issues",
                column: "refund_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_booking_outbox_messages_processed_at",
                schema: "booking",
                table: "outbox_messages",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_refund_invariant_issues",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "refund_success_inbox",
                schema: "booking");
        }
    }
}
