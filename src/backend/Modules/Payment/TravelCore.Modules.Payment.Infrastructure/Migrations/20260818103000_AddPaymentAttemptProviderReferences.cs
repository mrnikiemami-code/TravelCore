using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAttemptProviderReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provider_key",
                schema: "payment",
                table: "payment_attempts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_request_reference",
                schema: "payment",
                table: "payment_attempts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_transaction_reference",
                schema: "payment",
                table: "payment_attempts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_payment_attempts_provider_request",
                schema: "payment",
                table: "payment_attempts",
                columns: new[] { "provider_key", "provider_request_reference" },
                unique: true,
                filter: "provider_key IS NOT NULL AND provider_request_reference IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_payment_attempts_provider_transaction",
                schema: "payment",
                table: "payment_attempts",
                columns: new[] { "provider_key", "provider_transaction_reference" },
                unique: true,
                filter: "provider_key IS NOT NULL AND provider_transaction_reference IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_payment_attempts_provider_request",
                schema: "payment",
                table: "payment_attempts");

            migrationBuilder.DropIndex(
                name: "ux_payment_attempts_provider_transaction",
                schema: "payment",
                table: "payment_attempts");

            migrationBuilder.DropColumn(
                name: "provider_key",
                schema: "payment",
                table: "payment_attempts");

            migrationBuilder.DropColumn(
                name: "provider_request_reference",
                schema: "payment",
                table: "payment_attempts");

            migrationBuilder.DropColumn(
                name: "provider_transaction_reference",
                schema: "payment",
                table: "payment_attempts");
        }
    }
}
