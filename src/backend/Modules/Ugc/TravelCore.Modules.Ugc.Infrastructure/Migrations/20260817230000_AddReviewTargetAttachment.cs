using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Ugc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTargetAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "target_id",
                schema: "ugc",
                table: "reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "target_type",
                schema: "ugc",
                table: "reviews",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_target_type_target_id",
                schema: "ugc",
                table: "reviews",
                columns: new[] { "target_type", "target_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_reviews_target_type_target_id",
                schema: "ugc",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "target_id",
                schema: "ugc",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "target_type",
                schema: "ugc",
                table: "reviews");
        }
    }
}
