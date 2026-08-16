using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_variants",
                schema: "media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    byte_size = table.Column<long>(type: "bigint", nullable: true),
                    storage_key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_variants_media_assets",
                        column: x => x.media_asset_id,
                        principalSchema: "media",
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_media_variants_asset_profile",
                schema: "media",
                table: "media_variants",
                columns: new[] { "media_asset_id", "profile" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_media_variants_storage_key",
                schema: "media",
                table: "media_variants",
                column: "storage_key",
                unique: true,
                filter: "storage_key IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_variants",
                schema: "media");
        }
    }
}
