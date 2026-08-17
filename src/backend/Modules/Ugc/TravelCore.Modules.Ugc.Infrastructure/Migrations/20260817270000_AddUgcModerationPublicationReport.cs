using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Ugc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUgcModerationPublicationReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "moderation_status",
                schema: "ugc",
                table: "user_photos",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "publication_status",
                schema: "ugc",
                table: "user_photos",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Hidden");

            migrationBuilder.AddColumn<string>(
                name: "moderation_status",
                schema: "ugc",
                table: "travelogues",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "publication_status",
                schema: "ugc",
                table: "travelogues",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<string>(
                name: "moderation_status",
                schema: "ugc",
                table: "reviews",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "publication_status",
                schema: "ugc",
                table: "reviews",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Hidden");

            migrationBuilder.AddColumn<string>(
                name: "moderation_status",
                schema: "ugc",
                table: "comments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "publication_status",
                schema: "ugc",
                table: "comments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Hidden");

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "ugc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    optional_detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_moderation_status",
                schema: "ugc",
                table: "user_photos",
                column: "moderation_status");

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_publication_status",
                schema: "ugc",
                table: "user_photos",
                column: "publication_status");

            migrationBuilder.CreateIndex(
                name: "ix_travelogues_moderation_status",
                schema: "ugc",
                table: "travelogues",
                column: "moderation_status");

            migrationBuilder.CreateIndex(
                name: "ix_travelogues_publication_status",
                schema: "ugc",
                table: "travelogues",
                column: "publication_status");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_moderation_status",
                schema: "ugc",
                table: "reviews",
                column: "moderation_status");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_publication_status",
                schema: "ugc",
                table: "reviews",
                column: "publication_status");

            migrationBuilder.CreateIndex(
                name: "ix_comments_moderation_status",
                schema: "ugc",
                table: "comments",
                column: "moderation_status");

            migrationBuilder.CreateIndex(
                name: "ix_comments_publication_status",
                schema: "ugc",
                table: "comments",
                column: "publication_status");

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_actor_id",
                schema: "ugc",
                table: "reports",
                column: "reporter_actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_status",
                schema: "ugc",
                table: "reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_reports_target_type_target_id",
                schema: "ugc",
                table: "reports",
                columns: new[] { "target_type", "target_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reports",
                schema: "ugc");

            migrationBuilder.DropIndex(
                name: "ix_user_photos_moderation_status",
                schema: "ugc",
                table: "user_photos");

            migrationBuilder.DropIndex(
                name: "ix_user_photos_publication_status",
                schema: "ugc",
                table: "user_photos");

            migrationBuilder.DropIndex(
                name: "ix_travelogues_moderation_status",
                schema: "ugc",
                table: "travelogues");

            migrationBuilder.DropIndex(
                name: "ix_travelogues_publication_status",
                schema: "ugc",
                table: "travelogues");

            migrationBuilder.DropIndex(
                name: "ix_reviews_moderation_status",
                schema: "ugc",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "ix_reviews_publication_status",
                schema: "ugc",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "ix_comments_moderation_status",
                schema: "ugc",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_publication_status",
                schema: "ugc",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "moderation_status",
                schema: "ugc",
                table: "user_photos");

            migrationBuilder.DropColumn(
                name: "publication_status",
                schema: "ugc",
                table: "user_photos");

            migrationBuilder.DropColumn(
                name: "moderation_status",
                schema: "ugc",
                table: "travelogues");

            migrationBuilder.DropColumn(
                name: "publication_status",
                schema: "ugc",
                table: "travelogues");

            migrationBuilder.DropColumn(
                name: "moderation_status",
                schema: "ugc",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "publication_status",
                schema: "ugc",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "moderation_status",
                schema: "ugc",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "publication_status",
                schema: "ugc",
                table: "comments");
        }
    }
}
