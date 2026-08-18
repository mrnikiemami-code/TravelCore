using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelBookingPaymentTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_payments_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.AlterColumn<Guid>(
                name: "booking_id",
                schema: "payment",
                table: "refunds",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "hotel_booking_id",
                schema: "payment",
                table: "refunds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "booking_id",
                schema: "payment",
                table: "payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "hotel_booking_id",
                schema: "payment",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_refunds_exactly_one_target",
                schema: "payment",
                table: "refunds",
                sql: "(booking_id IS NOT NULL AND hotel_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ux_payments_booking_id",
                schema: "payment",
                table: "payments",
                column: "booking_id",
                unique: true,
                filter: "booking_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_payments_hotel_booking_id",
                schema: "payment",
                table: "payments",
                column: "hotel_booking_id",
                unique: true,
                filter: "hotel_booking_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_payments_exactly_one_target",
                schema: "payment",
                table: "payments",
                sql: "(booking_id IS NOT NULL AND hotel_booking_id IS NULL) OR (booking_id IS NULL AND hotel_booking_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_refunds_exactly_one_target",
                schema: "payment",
                table: "refunds");

            migrationBuilder.DropIndex(
                name: "ux_payments_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ux_payments_hotel_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropCheckConstraint(
                name: "ck_payments_exactly_one_target",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "hotel_booking_id",
                schema: "payment",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "hotel_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.AlterColumn<Guid>(
                name: "booking_id",
                schema: "payment",
                table: "refunds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "booking_id",
                schema: "payment",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_payments_booking_id",
                schema: "payment",
                table: "payments",
                column: "booking_id",
                unique: true);
        }
    }
}
