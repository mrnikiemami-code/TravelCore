using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicHotelBookingAccessAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "actor_account_id",
                schema: "hotel_booking",
                table: "hotel_bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hotel_booking_access_credentials",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_access_credentials", x => x.hotel_booking_id);
                    table.ForeignKey(
                        name: "FK_hotel_booking_access_credentials_hotel_bookings_hotel_booki~",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_booking_public_idempotency",
                schema: "hotel_booking",
                columns: table => new
                {
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_public_idempotency", x => x.idempotency_key);
                    table.ForeignKey(
                        name: "FK_hotel_booking_public_idempotency_hotel_bookings_hotel_booki~",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_access_credentials_token_hash",
                schema: "hotel_booking",
                table: "hotel_booking_access_credentials",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hotel_booking_public_idempotency_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_booking_public_idempotency",
                column: "hotel_booking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_booking_access_credentials",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_booking_public_idempotency",
                schema: "hotel_booking");

            migrationBuilder.DropColumn(
                name: "actor_account_id",
                schema: "hotel_booking",
                table: "hotel_bookings");
        }
    }
}
