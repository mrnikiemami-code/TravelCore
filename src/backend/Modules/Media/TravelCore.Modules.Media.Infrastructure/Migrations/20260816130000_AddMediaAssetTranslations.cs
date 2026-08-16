using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_asset_translations",
                schema: "media",
                columns: table => new
                {
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    caption = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    publication_status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_asset_translations", x => new { x.media_asset_id, x.locale_code });
                    table.ForeignKey(
                        name: "fk_media_asset_translations_media_assets",
                        column: x => x.media_asset_id,
                        principalSchema: "media",
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_media_asset_translations_asset_publication",
                schema: "media",
                table: "media_asset_translations",
                columns: new[] { "media_asset_id", "publication_status" });

            migrationBuilder.CreateIndex(
                name: "ix_media_asset_translations_locale_code",
                schema: "media",
                table: "media_asset_translations",
                column: "locale_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_asset_translations",
                schema: "media");
        }
    }
}
