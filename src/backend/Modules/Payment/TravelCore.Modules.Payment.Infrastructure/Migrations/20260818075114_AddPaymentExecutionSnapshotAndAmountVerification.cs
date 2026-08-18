using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentExecutionSnapshotAndAmountVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_payment_reconciliation_issues_kind",
                schema: "payment",
                table: "payment_reconciliation_issues");

            migrationBuilder.AddColumn<Guid>(
                name: "booking_snapshot_id",
                schema: "payment",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "execution_amount",
                schema: "payment",
                table: "payments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "execution_captured_at",
                schema: "payment",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "execution_currency",
                schema: "payment",
                table: "payments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_payment_reconciliation_issues_kind",
                schema: "payment",
                table: "payment_reconciliation_issues",
                sql: "kind IN (1, 2, 3, 4)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_payment_reconciliation_issues_kind",
                schema: "payment",
                table: "payment_reconciliation_issues");

            migrationBuilder.DropColumn(
                name: "booking_snapshot_id",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "execution_amount",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "execution_captured_at",
                schema: "payment",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "execution_currency",
                schema: "payment",
                table: "payments");

            migrationBuilder.AddCheckConstraint(
                name: "ck_payment_reconciliation_issues_kind",
                schema: "payment",
                table: "payment_reconciliation_issues",
                sql: "kind IN (1, 2)");
        }
    }
}
