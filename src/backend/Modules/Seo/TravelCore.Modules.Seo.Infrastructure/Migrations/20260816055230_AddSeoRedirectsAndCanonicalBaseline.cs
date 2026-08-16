using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Seo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoRedirectsAndCanonicalBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "activated_at",
                schema: "seo",
                table: "seo_redirect_candidates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "seo_redirects",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    seo_route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    from_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    to_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    source_candidate_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_redirects", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_seo_redirects_resource_locale",
                schema: "seo",
                table: "seo_redirects",
                columns: new[] { "resource_type", "resource_id", "locale" });

            migrationBuilder.CreateIndex(
                name: "ux_seo_redirects_locale_from_path",
                schema: "seo",
                table: "seo_redirects",
                columns: new[] { "locale", "from_path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seo_redirects",
                schema: "seo");

            migrationBuilder.DropColumn(
                name: "activated_at",
                schema: "seo",
                table: "seo_redirect_candidates");
        }
    }
}
