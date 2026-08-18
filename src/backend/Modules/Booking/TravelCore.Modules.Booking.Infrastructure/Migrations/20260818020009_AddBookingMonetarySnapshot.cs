using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingMonetarySnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "booking_monetary_snapshots",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_price_id = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    snapshot_target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quoted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    quote_expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_monetary_snapshots", x => x.id);
                    table.CheckConstraint("ck_booking_monetary_snapshots_quote_expires_after_quoted", "quote_expires_at > quoted_at");
                    table.ForeignKey(
                        name: "FK_booking_monetary_snapshots_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "booking",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_monetary_snapshot_components",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_monetary_snapshot_components", x => x.id);
                    table.ForeignKey(
                        name: "FK_booking_monetary_snapshot_components_booking_monetary_snaps~",
                        column: x => x.snapshot_id,
                        principalSchema: "booking",
                        principalTable: "booking_monetary_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_monetary_snapshot_components_snapshot_id",
                schema: "booking",
                table: "booking_monetary_snapshot_components",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ux_booking_monetary_snapshot_components_snapshot_sort",
                schema: "booking",
                table: "booking_monetary_snapshot_components",
                columns: new[] { "snapshot_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_booking_monetary_snapshots_booking_id",
                schema: "booking",
                table: "booking_monetary_snapshots",
                column: "booking_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_monetary_snapshot_components",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "booking_monetary_snapshots",
                schema: "booking");
        }
    }
}
