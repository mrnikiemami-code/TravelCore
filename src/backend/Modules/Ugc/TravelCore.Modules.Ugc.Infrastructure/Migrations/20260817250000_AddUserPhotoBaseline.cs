using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Ugc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPhotoBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_photos",
                schema: "ugc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_photos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_actor_id",
                schema: "ugc",
                table: "user_photos",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_photos_media_asset_id",
                schema: "ugc",
                table: "user_photos",
                column: "media_asset_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_photos",
                schema: "ugc");
        }
    }
}
