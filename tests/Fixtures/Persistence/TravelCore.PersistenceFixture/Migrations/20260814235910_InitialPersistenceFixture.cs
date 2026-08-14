using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace TravelCore.PersistenceFixture.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistenceFixture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "p01_fixture");

            migrationBuilder.CreateTable(
                name: "persistence_probes",
                schema: "p01_fixture",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstantValue = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LocalDateValue = table.Column<LocalDate>(type: "date", nullable: false),
                    LocalTimeValue = table.Column<LocalTime>(type: "time", nullable: false),
                    LocalDateTimeValue = table.Column<LocalDateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persistence_probes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "persistence_probes",
                schema: "p01_fixture");
        }
    }
}
