using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteRequestedDisplayCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "requested_display_currency",
                schema: "pricing",
                table: "quotes",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requested_display_currency",
                schema: "pricing",
                table: "quotes");
        }
    }
}
