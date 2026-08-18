using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelBookingCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_hotel_booking_reconciliation_issues_kind",
                schema: "hotel_booking",
                table: "hotel_booking_reconciliation_issues");

            migrationBuilder.CreateTable(
                name: "hotel_booking_cancellations",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    financial_outcome = table.Column<short>(type: "smallint", nullable: false),
                    penalty_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    completed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_cancellations", x => x.id);
                    table.CheckConstraint("ck_hotel_booking_cancellations_financial_outcome", "financial_outcome IN (1, 2)");
                    table.CheckConstraint("ck_hotel_booking_cancellations_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_hotel_booking_cancellations_hotel_bookings_hotel_booking_id",
                        column: x => x.hotel_booking_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_booking_cancellation_idempotency",
                schema: "hotel_booking",
                columns: table => new
                {
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    hotel_booking_cancellation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_supplier_cancellation_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_booking_cancellation_idempotency", x => new { x.hotel_booking_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_hotel_booking_cancellation_idempotency_hotel_booking_cancel~",
                        column: x => x.hotel_booking_cancellation_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_booking_cancellations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_supplier_cancellation_attempts",
                schema: "hotel_booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_cancellation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    initiated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotel_supplier_cancellation_attempts", x => x.id);
                    table.CheckConstraint("ck_hotel_supplier_cancellation_attempts_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_hotel_supplier_cancellation_attempts_hotel_booking_cancella~",
                        column: x => x.hotel_booking_cancellation_id,
                        principalSchema: "hotel_booking",
                        principalTable: "hotel_booking_cancellations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_hotel_booking_reconciliation_issues_kind",
                schema: "hotel_booking",
                table: "hotel_booking_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)");

            migrationBuilder.CreateIndex(
                name: "IX_hotel_booking_cancellation_idempotency_hotel_booking_cancel~",
                schema: "hotel_booking",
                table: "hotel_booking_cancellation_idempotency",
                column: "hotel_booking_cancellation_id");

            migrationBuilder.CreateIndex(
                name: "ux_hotel_booking_cancellations_hotel_booking_id",
                schema: "hotel_booking",
                table: "hotel_booking_cancellations",
                column: "hotel_booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_hotel_supplier_cancellation_attempts_one_unresolved",
                schema: "hotel_booking",
                table: "hotel_supplier_cancellation_attempts",
                column: "hotel_booking_cancellation_id",
                unique: true,
                filter: "status IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotel_booking_cancellation_idempotency",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_supplier_cancellation_attempts",
                schema: "hotel_booking");

            migrationBuilder.DropTable(
                name: "hotel_booking_cancellations",
                schema: "hotel_booking");

            migrationBuilder.DropCheckConstraint(
                name: "ck_hotel_booking_reconciliation_issues_kind",
                schema: "hotel_booking",
                table: "hotel_booking_reconciliation_issues");

            migrationBuilder.AddCheckConstraint(
                name: "ck_hotel_booking_reconciliation_issues_kind",
                schema: "hotel_booking",
                table: "hotel_booking_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4, 5, 6, 7, 8)");
        }
    }
}
