using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSourceContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "agency_offer_id",
                schema: "booking",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "agency_profile_id",
                schema: "booking",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "source_kind",
                schema: "booking",
                table: "bookings",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddCheckConstraint(
                name: "ck_bookings_source_context",
                schema: "booking",
                table: "bookings",
                sql: "(source_kind = 0 AND agency_profile_id IS NULL AND agency_offer_id IS NULL) OR (source_kind = 1 AND agency_profile_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_bookings_source_context",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "agency_offer_id",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "agency_profile_id",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "source_kind",
                schema: "booking",
                table: "bookings");
        }
    }
}
