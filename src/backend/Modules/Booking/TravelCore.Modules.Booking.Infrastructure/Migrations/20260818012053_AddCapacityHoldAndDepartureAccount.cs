using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityHoldAndDepartureAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "capacity_holds",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tour_departure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_count = table.Column<int>(type: "integer", nullable: false),
                    observed_configured_capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    status_changed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capacity_holds", x => x.id);
                    table.CheckConstraint("ck_capacity_holds_expires_after_created", "expires_at > created_at");
                    table.CheckConstraint("ck_capacity_holds_observed_capacity_positive", "observed_configured_capacity > 0");
                    table.CheckConstraint("ck_capacity_holds_seat_count_positive", "seat_count > 0");
                    table.ForeignKey(
                        name: "FK_capacity_holds_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "departure_capacity_accounts",
                schema: "booking",
                columns: table => new
                {
                    tour_departure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    active_seats = table.Column<int>(type: "integer", nullable: false),
                    consumed_seats = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departure_capacity_accounts", x => x.tour_departure_id);
                    table.CheckConstraint("ck_departure_capacity_accounts_active_seats_nonnegative", "active_seats >= 0");
                    table.CheckConstraint("ck_departure_capacity_accounts_consumed_seats_nonnegative", "consumed_seats >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_capacity_holds_tour_departure_id",
                schema: "booking",
                table: "capacity_holds",
                column: "tour_departure_id");

            migrationBuilder.CreateIndex(
                name: "ux_capacity_holds_idempotency_key",
                schema: "booking",
                table: "capacity_holds",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_capacity_holds_one_active_per_booking",
                schema: "booking",
                table: "capacity_holds",
                column: "booking_id",
                unique: true,
                filter: "status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capacity_holds",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "departure_capacity_accounts",
                schema: "booking");
        }
    }
}
