using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicBookingAccessAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "booking_access_credentials",
                schema: "booking",
                columns: table => new
                {
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_access_credentials", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_booking_access_credentials_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_public_idempotency",
                schema: "booking",
                columns: table => new
                {
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_public_idempotency", x => x.idempotency_key);
                    table.ForeignKey(
                        name: "FK_booking_public_idempotency_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_booking_access_credentials_token_hash",
                schema: "booking",
                table: "booking_access_credentials",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_booking_public_idempotency_booking_id",
                schema: "booking",
                table: "booking_public_idempotency",
                column: "booking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_access_credentials",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "booking_public_idempotency",
                schema: "booking");
        }
    }
}
