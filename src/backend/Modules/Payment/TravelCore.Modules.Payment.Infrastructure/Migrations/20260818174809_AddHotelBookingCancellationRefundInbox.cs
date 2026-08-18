using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelBookingCancellationRefundInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hotel_booking_cancellation_refund_inbox",
                schema: "payment",
                columns: table => new
                {
                    hotel_booking_cancellation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_cancellation_refund_inbox", x => x.hotel_booking_cancellation_id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_cancellation_refund_inbox_payment_id",
                schema: "payment",
                table: "hotel_booking_cancellation_refund_inbox",
                column: "payment_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_booking_cancellation_refund_inbox",
                schema: "payment");
        }
    }
}
