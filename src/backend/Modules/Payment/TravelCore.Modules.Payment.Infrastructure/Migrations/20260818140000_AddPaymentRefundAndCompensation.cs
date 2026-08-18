using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRefundAndCompensation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compensation_inbox",
                schema: "payment",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compensation_inbox", x => x.payment_id);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    status_changed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    succeeded_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.id);
                    table.CheckConstraint("ck_refunds_status", "status IN (1, 2)");
                    table.CheckConstraint("ck_refunds_version_nonnegative", "version >= 0");
                    table.ForeignKey(
                        name: "FK_refunds_payments_payment_id",
                        column: x => x.payment_id,
                        principalSchema: "payment",
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refund_attempts",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    initiated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    status_changed_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    provider_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    provider_request_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    provider_transaction_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_attempts", x => x.id);
                    table.CheckConstraint("ck_refund_attempts_status", "status IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_refund_attempts_refunds_refund_id",
                        column: x => x.refund_id,
                        principalSchema: "payment",
                        principalTable: "refunds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refund_reconciliation_issues",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_reconciliation_issues", x => x.id);
                    table.CheckConstraint("ck_refund_reconciliation_issues_kind", "kind IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_refund_reconciliation_issues_refunds_refund_id",
                        column: x => x.refund_id,
                        principalSchema: "payment",
                        principalTable: "refunds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_refund_attempts_one_active_per_refund",
                schema: "payment",
                table: "refund_attempts",
                column: "refund_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ux_refund_attempts_provider_request",
                schema: "payment",
                table: "refund_attempts",
                columns: new[] { "provider_key", "provider_request_reference" },
                unique: true,
                filter: "provider_key IS NOT NULL AND provider_request_reference IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_refund_attempts_provider_transaction",
                schema: "payment",
                table: "refund_attempts",
                columns: new[] { "provider_key", "provider_transaction_reference" },
                unique: true,
                filter: "provider_key IS NOT NULL AND provider_transaction_reference IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refund_reconciliation_issues_attempt_id",
                schema: "payment",
                table: "refund_reconciliation_issues",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_reconciliation_issues_refund_id",
                schema: "payment",
                table: "refund_reconciliation_issues",
                column: "refund_id");

            migrationBuilder.CreateIndex(
                name: "ux_refunds_payment_id",
                schema: "payment",
                table: "refunds",
                column: "payment_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compensation_inbox",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "refund_attempts",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "refund_reconciliation_issues",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "refunds",
                schema: "payment");
        }
    }
}
