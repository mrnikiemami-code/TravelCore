using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Seo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoPathHistoryAndReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "seo_path_history",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    seo_route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    succeeded_by_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    recorded_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_path_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seo_path_reservations",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    reserved_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_path_reservations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seo_redirect_candidates",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    seo_route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    from_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    to_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_redirect_candidates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_seo_path_history_resource_locale",
                schema: "seo",
                table: "seo_path_history",
                columns: new[] { "resource_type", "resource_id", "locale" });

            migrationBuilder.CreateIndex(
                name: "ix_seo_path_history_route",
                schema: "seo",
                table: "seo_path_history",
                column: "seo_route_id");

            migrationBuilder.CreateIndex(
                name: "ix_seo_path_reservations_resource",
                schema: "seo",
                table: "seo_path_reservations",
                columns: new[] { "resource_type", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ux_seo_path_reservations_locale_path",
                schema: "seo",
                table: "seo_path_reservations",
                columns: new[] { "locale", "path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seo_redirect_candidates_resource_locale",
                schema: "seo",
                table: "seo_redirect_candidates",
                columns: new[] { "resource_type", "resource_id", "locale" });

            migrationBuilder.CreateIndex(
                name: "ix_seo_redirect_candidates_route",
                schema: "seo",
                table: "seo_redirect_candidates",
                column: "seo_route_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seo_path_history",
                schema: "seo");

            migrationBuilder.DropTable(
                name: "seo_path_reservations",
                schema: "seo");

            migrationBuilder.DropTable(
                name: "seo_redirect_candidates",
                schema: "seo");
        }
    }
}
