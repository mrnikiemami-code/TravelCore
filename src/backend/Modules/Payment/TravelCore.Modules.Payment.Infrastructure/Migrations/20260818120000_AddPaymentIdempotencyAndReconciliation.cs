using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIdempotencyAndReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payment_attempts_payment_id",
                schema: "payment",
                table: "payment_attempts");

            migrationBuilder.AddColumn<long>(
                name: "version",
                schema: "payment",
                table: "payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddCheckConstraint(
                name: "ck_payments_version_nonnegative",
                schema: "payment",
                table: "payments",
                sql: "version >= 0");

            migrationBuilder.CreateTable(
                name: "payment_initiation_idempotency",
                schema: "payment",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_initiation_idempotency", x => new { x.payment_id, x.idempotency_key });
                    table.ForeignKey(
                        name: "FK_payment_initiation_idempotency_payments_payment_id",
                        column: x => x.payment_id,
                        principalSchema: "payment",
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_reconciliation_issues",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_reconciliation_issues", x => x.id);
                    table.CheckConstraint("ck_payment_reconciliation_issues_kind", "kind IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_payment_reconciliation_issues_payments_payment_id",
                        column: x => x.payment_id,
                        principalSchema: "payment",
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_payments_booking_id",
                schema: "payment",
                table: "payments",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_payment_attempts_one_active_per_payment",
                schema: "payment",
                table: "payment_attempts",
                column: "payment_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ix_payment_initiation_idempotency_attempt_id",
                schema: "payment",
                table: "payment_initiation_idempotency",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_reconciliation_issues_attempt_id",
                schema: "payment",
                table: "payment_reconciliation_issues",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_reconciliation_issues_payment_id",
                schema: "payment",
                table: "payment_reconciliation_issues",
                column: "payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_initiation_idempotency",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "payment_reconciliation_issues",
                schema: "payment");

            migrationBuilder.DropIndex(
                name: "ux_payments_booking_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ux_payment_attempts_one_active_per_payment",
                schema: "payment",
                table: "payment_attempts");

            migrationBuilder.DropCheckConstraint(
                name: "ck_payments_version_nonnegative",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "version",
                schema: "payment",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                schema: "payment",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_attempts_payment_id",
                schema: "payment",
                table: "payment_attempts",
                column: "payment_id");
        }
    }
}
