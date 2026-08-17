using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteAndPriceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quotes",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_price_id = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    snapshot_target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quote_snapshot_components",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quote_snapshot_components", x => x.id);
                    table.ForeignKey(
                        name: "FK_quote_snapshot_components_quotes_quote_id",
                        column: x => x.quote_id,
                        principalSchema: "pricing",
                        principalTable: "quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_quote_snapshot_components_quote_code",
                schema: "pricing",
                table: "quote_snapshot_components",
                columns: new[] { "quote_id", "code" },
                unique: true,
                filter: "code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_quote_snapshot_components_quote_sort_order",
                schema: "pricing",
                table: "quote_snapshot_components",
                columns: new[] { "quote_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_expires_at",
                schema: "pricing",
                table: "quotes",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_source_price_id",
                schema: "pricing",
                table: "quotes",
                column: "source_price_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quote_snapshot_components",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "quotes",
                schema: "pricing");
        }
    }
}
