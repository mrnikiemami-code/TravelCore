using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.DynamicPackage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageCompositionBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "package_compositions",
                schema: "dynamic_package",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_booking_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_compositions", x => x.id);
                    table.CheckConstraint(
                        "ck_package_compositions_refs_required",
                        "flight_booking_id IS NOT NULL AND hotel_booking_id IS NOT NULL");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "package_compositions",
                schema: "dynamic_package");
        }
    }
}
