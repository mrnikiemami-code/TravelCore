using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Seo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoIndexPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "seo_index_policies",
                schema: "seo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<short>(type: "smallint", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    index_directive = table.Column<short>(type: "smallint", nullable: false),
                    follow_directive = table.Column<short>(type: "smallint", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_index_policies", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_seo_index_policies_resource_locale",
                schema: "seo",
                table: "seo_index_policies",
                columns: new[] { "resource_type", "resource_id", "locale" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seo_index_policies",
                schema: "seo");
        }
    }
}
