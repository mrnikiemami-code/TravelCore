using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetFocalPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "focal_x",
                schema: "media",
                table: "media_assets",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "focal_y",
                schema: "media",
                table: "media_assets",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "focal_x",
                schema: "media",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "focal_y",
                schema: "media",
                table: "media_assets");
        }
    }
}
