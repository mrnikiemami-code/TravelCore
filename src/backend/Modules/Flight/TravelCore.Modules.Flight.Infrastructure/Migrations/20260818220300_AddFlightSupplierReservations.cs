using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Flight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightSupplierReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flight_reconciliation_issues",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_supplier_reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    flight_supplier_reservation_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    detail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_reconciliation_issues", x => x.id);
                    table.CheckConstraint("ck_flight_reconciliation_issues_kind", "kind IN (1, 2, 3, 4, 5, 6, 7)");
                    table.ForeignKey(
                        name: "FK_flight_reconciliation_issues_flight_bookings_flight_booking~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_supplier_reservations",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    source_reservation_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    reservation_locator = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    reservation_expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_supplier_reservations", x => x.id);
                    table.CheckConstraint("ck_flight_supplier_reservations_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_flight_supplier_reservations_flight_bookings_flight_booking~",
                        column: x => x.flight_booking_id,
                        principalSchema: "flight",
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_supplier_reservation_attempts",
                schema: "flight",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_supplier_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    initiated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_supplier_reservation_attempts", x => x.id);
                    table.CheckConstraint("ck_flight_supplier_reservation_attempts_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_flight_supplier_reservation_attempts_flight_supplier_reserv~",
                        column: x => x.flight_supplier_reservation_id,
                        principalSchema: "flight",
                        principalTable: "flight_supplier_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_supplier_reservation_idempotency",
                schema: "flight",
                columns: table => new
                {
                    flight_supplier_reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    flight_supplier_reservation_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_supplier_reservation_idempotency", x => new { x.flight_supplier_reservation_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_flight_supplier_reservation_idempotency_flight_supplier_res~",
                        column: x => x.flight_supplier_reservation_id,
                        principalSchema: "flight",
                        principalTable: "flight_supplier_reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_flight_reconciliation_issues_flight_booking_id",
                schema: "flight",
                table: "flight_reconciliation_issues",
                column: "flight_booking_id");

            migrationBuilder.CreateIndex(
                name: "ux_flight_supplier_reservation_attempts_one_unresolved",
                schema: "flight",
                table: "flight_supplier_reservation_attempts",
                column: "flight_supplier_reservation_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ux_flight_supplier_reservations_flight_booking_id",
                schema: "flight",
                table: "flight_supplier_reservations",
                column: "flight_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_flight_supplier_reservations_source_ref",
                schema: "flight",
                table: "flight_supplier_reservations",
                columns: new[] { "source_key", "source_reservation_reference" },
                unique: true,
                filter: "source_reservation_reference IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_reconciliation_issues",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_supplier_reservation_attempts",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_supplier_reservation_idempotency",
                schema: "flight");

            migrationBuilder.DropTable(
                name: "flight_supplier_reservations",
                schema: "flight");
        }
    }
}
