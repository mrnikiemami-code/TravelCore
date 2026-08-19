using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Flight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicFlightBookingAccessAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "actor_account_id",
                schema: "flight",
                table: "flight_bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "flight_booking_access_credentials",
                schema: "flight",
                columns: table => new
                {
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_access_credentials", x => x.flight_booking_id);
                    table.ForeignKey(
                        name: "FK_flight_booking_access_credentials_flight_bookings_flight_bo~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_booking_public_idempotency",
                schema: "flight",
                columns: table => new
                {
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_public_idempotency", x => x.idempotency_key);
                    table.ForeignKey(
                        name: "FK_flight_booking_public_idempotency_flight_bookings_flight_bo~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_flight_booking_access_credentials_token_hash",
                schema: "flight",
                table: "flight_booking_access_credentials",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flight_booking_public_idempotency_flight_booking_id",
                schema: "flight",
                table: "flight_booking_public_idempotency",
                column: "flight_booking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_booking_access_credentials",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_booking_public_idempotency",
                schema: "flight");

            migrationBuilder.DropColumn(
                name: "actor_account_id",
                schema: "flight",
                table: "flight_bookings");
        }
    }
}
