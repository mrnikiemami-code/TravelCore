using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightBookingPaymentTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_refunds_exactly_one_target",
                schema: "payment",
                table: "refunds");

            migrationBuilder.DropCheckConstraint(
                name: "ck_payments_exactly_one_target",
                schema: "payment",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "flight_booking_id",
                schema: "payment",
                table: "refunds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "flight_booking_id",
                schema: "payment",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_refunds_exactly_one_target",
                schema: "payment",
                table: "refunds",
                sql: "(booking_id IS NOT NULL AND hotel_booking_id IS NULL AND flight_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL AND flight_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NULL AND flight_booking_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ux_payments_flight_booking_id",
                schema: "payment",
                table: "payments",
                column: "flight_booking_id",
                unique: true,
                filter: "flight_booking_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_payments_exactly_one_target",
                schema: "payment",
                table: "payments",
                sql: "(booking_id IS NOT NULL AND hotel_booking_id IS NULL AND flight_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL AND flight_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NULL AND flight_booking_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_refunds_exactly_one_target",
                schema: "payment",
                table: "refunds");

            migrationBuilder.DropIndex(
                name: "ux_payments_flight_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropCheckConstraint(
                name: "ck_payments_exactly_one_target",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "flight_booking_id",
                schema: "payment",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "flight_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.AddCheckConstraint(
                name: "ck_refunds_exactly_one_target",
                schema: "payment",
                table: "refunds",
                sql: "(booking_id IS NOT NULL AND hotel_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_payments_exactly_one_target",
                schema: "payment",
                table: "payments",
                sql: "(booking_id IS NOT NULL AND hotel_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL)");
        }
    }
}
