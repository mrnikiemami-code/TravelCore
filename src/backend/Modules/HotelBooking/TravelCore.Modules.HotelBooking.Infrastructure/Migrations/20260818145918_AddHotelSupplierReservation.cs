using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelSupplierReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "confirmed_at",
                schema: "hotel_booking",
                table: "hotel_bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "status",
                schema: "hotel_booking",
                table: "hotel_bookings",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            migrationBuilder.CreateTable(
                name: "hotel_booking_reconciliation_issues",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_supplier_reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hotel_supplier_reservation_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    detail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_reconciliation_issues", x => x.id);
                    table.CheckConstraint("ck_hotel_booking_reconciliation_issues_kind", "kind IN (1, 2, 3, 4, 5, 6, 7, 8)");
                    table.ForeignKey(
                        name: "FK_hotel_booking_reconciliation_issues_hotel_bookings_hotel_bo~",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_supplier_reservations",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    source_reservation_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    supplier_confirmation_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_supplier_reservations", x => x.id);
                    table.CheckConstraint("ck_hotel_supplier_reservations_status", "status IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_hotel_supplier_reservations_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_supplier_reservation_attempts",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_supplier_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    initiated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_supplier_reservation_attempts", x => x.id);
                    table.CheckConstraint("ck_hotel_supplier_reservation_attempts_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_hotel_supplier_reservation_attempts_hotel_supplier_reservat~",
                        column: x => x.hotel_supplier_reservation_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_supplier_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_supplier_reservation_idempotency",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_supplier_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    hotel_supplier_reservation_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_supplier_reservation_idempotency", x => new { x.hotel_supplier_reservation_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_hotel_supplier_reservation_idempotency_hotel_supplier_reser~",
                        column: x => x.hotel_supplier_reservation_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_supplier_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_hotel_bookings_status",
                schema: "hotel_booking",
                table: "hotel_bookings",
                sql: "status IN (1, 2, 3)");

            migrationBuilder.CreateIndex(
                name: "ix_hotel_booking_reconciliation_issues_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_booking_reconciliation_issues",
                column: "hotel_booking_id");

            migrationBuilder.CreateIndex(
                name: "ux_hotel_supplier_reservation_attempts_one_unresolved",
                schema: "hotel_booking",
                table: "hotel_supplier_reservation_attempts",
                column: "hotel_supplier_reservation_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ux_hotel_supplier_reservations_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_supplier_reservations",
                column: "hotel_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_hotel_supplier_reservations_source_ref",
                schema: "hotel_booking",
                table: "hotel_supplier_reservations",
                columns: new[] { "source_key", "source_reservation_reference" },
                unique: true,
                filter: "source_reservation_reference IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_booking_reconciliation_issues",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_supplier_reservation_attempts",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_supplier_reservation_idempotency",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_supplier_reservations",
                schema: "hotel_booking");

            migrationBuilder.DropCheckConstraint(
                name: "ck_hotel_bookings_status",
                schema: "hotel_booking",
                table: "hotel_bookings");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                schema: "hotel_booking",
                table: "hotel_bookings");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "hotel_booking",
                table: "hotel_bookings");
        }
    }
}
