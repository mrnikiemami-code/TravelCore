using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingConfirmationRecoveryIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "booking_confirmation_recovery_issues",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_confirmation_recovery_issues", x => x.id);
                    table.CheckConstraint("ck_booking_confirmation_recovery_issues_reason", "reason IN (1, 2, 3, 4, 5, 6)");
                    table.ForeignKey(
                        name: "FK_booking_confirmation_recovery_issues_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_booking_confirmation_recovery_issues_booking_id",
                schema: "booking",
                table: "booking_confirmation_recovery_issues",
                column: "booking_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_confirmation_recovery_issues",
                schema: "booking");
        }
    }
}
