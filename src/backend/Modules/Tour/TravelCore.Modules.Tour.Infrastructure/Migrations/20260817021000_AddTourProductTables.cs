using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourProductTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tour");

            migrationBuilder.CreateTable(
                name: "tour_products",
                schema: "tour",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<short>(type: "smallint", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_products", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tour_products_created_at",
                schema: "tour",
                table: "tour_products",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tour_products_kind",
                schema: "tour",
                table: "tour_products",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "ux_tour_products_code",
                schema: "tour",
                table: "tour_products",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_products",
                schema: "tour");
        }
    }
}
