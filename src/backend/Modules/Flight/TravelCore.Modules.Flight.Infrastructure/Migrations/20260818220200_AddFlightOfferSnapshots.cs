using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Flight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightOfferSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_offer_idempotency",
                schema: "flight",
                columns: table => new
                {
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    flight_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_offer_idempotency", x => new { x.flight_booking_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_flight_offer_idempotency_flight_bookings_flight_booking_id",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_offer_snapshots",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_type = table.Column<short>(type: "smallint", nullable: false),
                    source_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_offer_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quoted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    offer_expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    cabin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    booking_class = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    fare_basis = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    fare_family = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_offer_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_flight_offer_snapshots_flight_bookings_flight_booking_id",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_booking_monetary_snapshots",
                schema: "flight",
                columns: table => new
                {
                    flight_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_fare_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    base_fare_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    taxes_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    taxes_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    fees_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    fees_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_booking_monetary_snapshots", x => x.flight_offer_snapshot_id);
                    table.ForeignKey(
                        name: "FK_flight_booking_monetary_snapshots_flight_offer_snapshots_fl~",
                        column: x => x.flight_offer_snapshot_id,
                        principalSchema: "flight",
                        principalTable: "flight_offer_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_fare_rule_snapshots",
                schema: "flight",
                columns: table => new
                {
                    flight_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refundable = table.Column<bool>(type: "boolean", nullable: false),
                    changeable = table.Column<bool>(type: "boolean", nullable: false),
                    ticketing_deadline = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    cancel_penalty_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    cancel_penalty_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    change_penalty_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    change_penalty_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    partial_refund_required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_fare_rule_snapshots", x => x.flight_offer_snapshot_id);
                    table.ForeignKey(
                        name: "FK_flight_fare_rule_snapshots_flight_offer_snapshots_flight_of~",
                        column: x => x.flight_offer_snapshot_id,
                        principalSchema: "flight",
                        principalTable: "flight_offer_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_passenger_category_fare_snapshots",
                schema: "flight",
                columns: table => new
                {
                    flight_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<short>(type: "smallint", nullable: false),
                    passenger_count = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_passenger_category_fare_snapshots", x => new { x.flight_offer_snapshot_id, x.ordinal });
                    table.CheckConstraint("ck_flight_passenger_category_fare_snapshots_category", "category IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_flight_passenger_category_fare_snapshots_flight_booking_mon~",
                        column: x => x.flight_offer_snapshot_id,
                        principalSchema: "flight",
                        principalTable: "flight_booking_monetary_snapshots",
                        principalColumn: "flight_offer_snapshot_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_baggage_allowance_snapshots",
                schema: "flight",
                columns: table => new
                {
                    flight_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    weight = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    unit = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    passenger_category = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_baggage_allowance_snapshots", x => new { x.flight_offer_snapshot_id, x.ordinal });
                    table.ForeignKey(
                        name: "FK_flight_baggage_allowance_snapshots_flight_fare_rule_snapsho~",
                        column: x => x.flight_offer_snapshot_id,
                        principalSchema: "flight",
                        principalTable: "flight_fare_rule_snapshots",
                        principalColumn: "flight_offer_snapshot_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_flight_booking_monetary_snapshots_flight_booking_id",
                schema: "flight",
                table: "flight_booking_monetary_snapshots",
                column: "flight_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_offer_snapshots_flight_booking_id",
                schema: "flight",
                table: "flight_offer_snapshots",
                column: "flight_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_offer_snapshots_source_offer",
                schema: "flight",
                table: "flight_offer_snapshots",
                columns: new[] { "source_key", "source_offer_reference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_baggage_allowance_snapshots",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_offer_idempotency",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_passenger_category_fare_snapshots",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_fare_rule_snapshots",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_booking_monetary_snapshots",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_offer_snapshots",
                schema: "flight");
        }
    }
}
