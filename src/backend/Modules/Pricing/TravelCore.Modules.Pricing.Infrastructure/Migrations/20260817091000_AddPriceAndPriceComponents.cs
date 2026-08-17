using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceAndPriceComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prices",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "price_components",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(24,8)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_components", x => x.id);
                    table.ForeignKey(
                        name: "FK_price_components_prices_price_id",
                        column: x => x.price_id,
                        principalSchema: "pricing",
                        principalTable: "prices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_price_components_price_code",
                schema: "pricing",
                table: "price_components",
                columns: new[] { "price_id", "code" },
                unique: true,
                filter: "code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_price_components_price_sort_order",
                schema: "pricing",
                table: "price_components",
                columns: new[] { "price_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prices_target_type_target_id",
                schema: "pricing",
                table: "prices",
                columns: new[] { "target_type", "target_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_components",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "prices",
                schema: "pricing");
        }
    }
}
