using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelBookingStayStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hotel_booking");

            migrationBuilder.CreateTable(
                name: "hotel_bookings",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_in_date = table.Column<LocalDate>(type: "date", nullable: false),
                    check_out_date = table.Column<LocalDate>(type: "date", nullable: false),
                    contact_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    contact_normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_bookings", x => x.id);
                    table.CheckConstraint("ck_hotel_bookings_checkout_after_checkin", "check_out_date > check_in_date");
                });

            migrationBuilder.CreateTable(
                name: "room_reservations",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_reservations_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_booking_guests",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<short>(type: "smallint", nullable: false),
                    age_at_check_in = table.Column<int>(type: "integer", nullable: true),
                    is_lead_guest = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_guests", x => x.id);
                    table.CheckConstraint("ck_hotel_booking_guests_age_by_category", "(category = 1 AND age_at_check_in IS NULL) OR (category = 2 AND age_at_check_in IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_hotel_booking_guests_room_reservations_room_reservation_id",
                        column: x => x.room_reservation_id,
                        principalSchema: "hotel_booking",
                        principalTable: "room_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hotel_booking_guests_room_reservation_id",
                schema: "hotel_booking",
                table: "hotel_booking_guests",
                column: "room_reservation_id");

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_guests_one_lead",
                schema: "hotel_booking",
                table: "hotel_booking_guests",
                column: "hotel_booking_id",
                unique: true,
                filter: "is_lead_guest = TRUE");

            migrationBuilder.CreateIndex(
                name: "ux_room_reservations_booking_ordinal",
                schema: "hotel_booking",
                table: "room_reservations",
                columns: new[] { "hotel_booking_id", "ordinal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_booking_guests",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "room_reservations",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_bookings",
                schema: "hotel_booking");
        }
    }
}
