using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Flight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightBookingItinerary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "flight");

            migrationBuilder.CreateTable(
                name: "flight_bookings",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_type = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_bookings", x => x.id);
                    table.CheckConstraint("ck_flight_bookings_trip_type", "trip_type IN (1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "flight_journeys",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_journeys", x => x.id);
                    table.ForeignKey(
                        name: "FK_flight_journeys_flight_bookings_flight_booking_id",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_passengers",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_passengers", x => x.id);
                    table.CheckConstraint("ck_flight_passengers_category", "category IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_flight_passengers_flight_bookings_flight_booking_id",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_segments",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_journey_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    origin_airport_iata = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    destination_airport_iata = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    departure_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    departure_time_zone_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    arrival_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    arrival_time_zone_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    marketing_carrier_iata = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    operating_carrier_iata = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    flight_number = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_segments", x => x.id);
                    table.CheckConstraint("ck_flight_segments_arrival_after_departure", "arrival_at > departure_at");
                    table.CheckConstraint("ck_flight_segments_origin_destination_differ", "origin_airport_iata <> destination_airport_iata");
                    table.ForeignKey(
                        name: "FK_flight_segments_flight_journeys_flight_journey_id",
                        column: x => x.flight_journey_id,
                        principalSchema: "flight",
                        principalTable: "flight_journeys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_flight_journeys_booking_ordinal",
                schema: "flight",
                table: "flight_journeys",
                columns: new[] { "flight_booking_id", "ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_passengers_booking_ordinal",
                schema: "flight",
                table: "flight_passengers",
                columns: new[] { "flight_booking_id", "ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_segments_journey_ordinal",
                schema: "flight",
                table: "flight_segments",
                columns: new[] { "flight_journey_id", "ordinal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_passengers",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_segments",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_journeys",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_bookings",
                schema: "flight");
        }
    }
}
