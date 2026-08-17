using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.Modules.Ugc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewRatingBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ugc");

            migrationBuilder.CreateTable(
                name: "reviews",
                schema: "ugc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    overall_rating = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "review_dimension_ratings",
                schema: "ugc",
                columns: table => new
                {
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    value = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_dimension_ratings", x => new { x.review_id, x.dimension_code });
                    table.ForeignKey(
                        name: "FK_review_dimension_ratings_reviews_review_id",
                        column: x => x.review_id,
                        principalSchema: "ugc",
                        principalTable: "reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_actor_id",
                schema: "ugc",
                table: "reviews",
                column: "actor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_dimension_ratings",
                schema: "ugc");

            migrationBuilder.DropTable(
                name: "reviews",
                schema: "ugc");
        }
    }
}
