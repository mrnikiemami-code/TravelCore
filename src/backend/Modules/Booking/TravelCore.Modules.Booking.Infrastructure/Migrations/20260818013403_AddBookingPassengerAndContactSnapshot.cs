using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPassengerAndContactSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "actor_reference_id",
                schema: "booking",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_display_name",
                schema: "booking",
                table: "bookings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_email",
                schema: "booking",
                table: "bookings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_normalized_email",
                schema: "booking",
                table: "bookings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_phone",
                schema: "booking",
                table: "bookings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "party_reference_id",
                schema: "booking",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "booking_passengers",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    traveler_category = table.Column<int>(type: "integer", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_passengers", x => x.id);
                    table.ForeignKey(
                        name: "FK_booking_passengers_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_passengers_booking_id",
                schema: "booking",
                table: "booking_passengers",
                column: "booking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_passengers",
                schema: "booking");

            migrationBuilder.DropColumn(
                name: "actor_reference_id",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "contact_display_name",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "contact_email",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "contact_normalized_email",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "contact_phone",
                schema: "booking",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "party_reference_id",
                schema: "booking",
                table: "bookings");
        }
    }
}
