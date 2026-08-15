using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.ReferenceData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialReferenceDataPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reference_data");

            migrationBuilder.CreateTable(
                name: "countries",
                schema: "reference_data",
                columns: table => new
                {
                    alpha2_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    alpha3_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    numeric_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    english_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.alpha2_code);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                schema: "reference_data",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    english_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    minor_units = table.Column<int>(type: "integer", nullable: false),
                    symbol = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "locales",
                schema: "reference_data",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    english_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locales", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "time_zones",
                schema: "reference_data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_zones", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_countries_alpha3_code",
                schema: "reference_data",
                table: "countries",
                column: "alpha3_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "countries",
                schema: "reference_data");

            migrationBuilder.DropTable(
                name: "currencies",
                schema: "reference_data");

            migrationBuilder.DropTable(
                name: "locales",
                schema: "reference_data");

            migrationBuilder.DropTable(
                name: "time_zones",
                schema: "reference_data");
        }
    }
}
