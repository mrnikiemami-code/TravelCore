using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Seo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoMetadataOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "seo_metadata_overrides",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    title_override = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description_override = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_metadata_overrides", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_seo_metadata_overrides_resource_locale",
                schema: "seo",
                table: "seo_metadata_overrides",
                columns: new[] { "resource_type", "resource_id", "locale" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seo_metadata_overrides",
                schema: "seo");
        }
    }
}
