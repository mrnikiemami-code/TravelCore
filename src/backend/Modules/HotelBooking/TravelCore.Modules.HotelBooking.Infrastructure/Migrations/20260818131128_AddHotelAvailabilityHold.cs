using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelAvailabilityHold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hotel_availability_holds",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    source_hold_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    requested_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    activated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    released_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_availability_holds", x => x.id);
                    table.CheckConstraint("ck_hotel_availability_holds_active_expiry", "(status <> 2) OR (expires_at IS NOT NULL)");
                    table.CheckConstraint("ck_hotel_availability_holds_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_hotel_availability_holds_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_hold_idempotency",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    hotel_availability_hold_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_hold_idempotency", x => new { x.hotel_booking_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_hotel_hold_idempotency_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_availability_hold_rooms",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_availability_hold_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selection_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_availability_hold_rooms", x => new { x.hotel_availability_hold_id, x.room_reservation_id });
                    table.ForeignKey(
                        name: "FK_hotel_availability_hold_rooms_hotel_availability_holds_hote~",
                        column: x => x.hotel_availability_hold_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_availability_holds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_hotel_availability_holds_one_unresolved",
                schema: "hotel_booking",
                table: "hotel_availability_holds",
                column: "hotel_booking_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ux_hotel_availability_holds_source_ref",
                schema: "hotel_booking",
                table: "hotel_availability_holds",
                columns: new[] { "source_key", "source_hold_reference" },
                unique: true,
                filter: "source_hold_reference IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_availability_hold_rooms",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_hold_idempotency",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_availability_holds",
                schema: "hotel_booking");
        }
    }
}
