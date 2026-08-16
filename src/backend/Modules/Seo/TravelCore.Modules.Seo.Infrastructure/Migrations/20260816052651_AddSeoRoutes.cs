using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Seo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "seo");

            migrationBuilder.CreateTable(
                name: "seo_routes",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_routes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_seo_routes_resource",
                schema: "seo",
                table: "seo_routes",
                columns: new[] { "resource_type", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ux_seo_routes_locale_path",
                schema: "seo",
                table: "seo_routes",
                columns: new[] { "locale", "path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_seo_routes_resource_locale",
                schema: "seo",
                table: "seo_routes",
                columns: new[] { "resource_type", "resource_id", "locale" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seo_routes",
                schema: "seo");
        }
    }
}
