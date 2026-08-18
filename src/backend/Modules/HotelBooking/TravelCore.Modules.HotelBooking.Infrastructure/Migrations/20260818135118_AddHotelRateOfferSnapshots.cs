using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelRateOfferSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hotel_rate_offer_idempotency",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    hotel_rate_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_rate_offer_idempotency", x => new { x.hotel_booking_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_hotel_rate_offer_idempotency_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_rate_offer_snapshots",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_in_date = table.Column<LocalDate>(type: "date", nullable: false),
                    check_out_date = table.Column<LocalDate>(type: "date", nullable: false),
                    source_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_offer_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quoted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    offer_expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    accepted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_rate_offer_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_hotel_rate_offer_snapshots_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_booking_monetary_snapshots",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_rate_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    payable_now_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    payable_now_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    payable_at_property_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    payable_at_property_currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_monetary_snapshots", x => x.hotel_rate_offer_snapshot_id);
                    table.ForeignKey(
                        name: "FK_hotel_booking_monetary_snapshots_hotel_rate_offer_snapshots~",
                        column: x => x.hotel_rate_offer_snapshot_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_rate_offer_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_cancellation_policy_snapshots",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_rate_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_time_zone_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    public_explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_cancellation_policy_snapshots", x => x.hotel_rate_offer_snapshot_id);
                    table.ForeignKey(
                        name: "FK_hotel_cancellation_policy_snapshots_hotel_rate_offer_snapsh~",
                        column: x => x.hotel_rate_offer_snapshot_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_rate_offer_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_room_rate_snapshots",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_rate_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: true),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    availability_selection_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    source_rate_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    board_basis_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_room_rate_snapshots", x => new { x.hotel_rate_offer_snapshot_id, x.room_reservation_id });
                    table.ForeignKey(
                        name: "FK_hotel_room_rate_snapshots_hotel_rate_offer_snapshots_hotel_~",
                        column: x => x.hotel_rate_offer_snapshot_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_rate_offer_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_charge_component_snapshots",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_rate_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_charge_component_snapshots", x => new { x.hotel_rate_offer_snapshot_id, x.ordinal });
                    table.ForeignKey(
                        name: "FK_hotel_charge_component_snapshots_hotel_booking_monetary_sna~",
                        column: x => x.hotel_rate_offer_snapshot_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_booking_monetary_snapshots",
                        principalColumn: "hotel_rate_offer_snapshot_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_cancellation_penalty_rules",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_rate_offer_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordinal = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    effective_until = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    penalty_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_cancellation_penalty_rules", x => new { x.hotel_rate_offer_snapshot_id, x.ordinal });
                    table.ForeignKey(
                        name: "FK_hotel_cancellation_penalty_rules_hotel_cancellation_policy_~",
                        column: x => x.hotel_rate_offer_snapshot_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_cancellation_policy_snapshots",
                        principalColumn: "hotel_rate_offer_snapshot_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_monetary_snapshots_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_booking_monetary_snapshots",
                column: "hotel_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_hotel_rate_offer_snapshots_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_rate_offer_snapshots",
                column: "hotel_booking_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_cancellation_penalty_rules",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_charge_component_snapshots",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_rate_offer_idempotency",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_room_rate_snapshots",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_cancellation_policy_snapshots",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_booking_monetary_snapshots",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_rate_offer_snapshots",
                schema: "hotel_booking");
        }
    }
}
