using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightBookingCancellationRefundInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_booking_cancellation_refund_inbox",
                schema: "payment",
                columns: table => new
                {
                    flight_booking_cancellation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_cancellation_refund_inbox", x => x.flight_booking_cancellation_id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_flight_booking_cancellation_refund_inbox_payment_id",
                schema: "payment",
                table: "flight_booking_cancellation_refund_inbox",
                column: "payment_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_booking_cancellation_refund_inbox",
                schema: "payment");
        }
    }
}
