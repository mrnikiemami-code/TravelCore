using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceOccupancyRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "price_occupancy_rules",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_id = table.Column<Guid>(type: "uuid", nullable: false),
                    market_price_type = table.Column<short>(type: "smallint", nullable: false),
                    passenger_category = table.Column<short>(type: "smallint", nullable: false),
                    occupancy_category = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_occupancy_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_price_occupancy_rules_prices_price_id",
                        column: x => x.price_id,
                        principalSchema: "pricing",
                        principalTable: "prices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_price_occupancy_rules_price_market_passenger_occupancy",
                schema: "pricing",
                table: "price_occupancy_rules",
                columns: new[] { "price_id", "market_price_type", "passenger_category", "occupancy_category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_price_occupancy_rules_price_sort_order",
                schema: "pricing",
                table: "price_occupancy_rules",
                columns: new[] { "price_id", "sort_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_occupancy_rules",
                schema: "pricing");
        }
    }
}
