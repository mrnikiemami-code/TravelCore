using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Tour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourProductAgencyLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "agency_id",
                schema: "tour",
                table: "tour_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tour_products_agency_id",
                schema: "tour",
                table: "tour_products",
                column: "agency_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tour_products_agency_id",
                schema: "tour",
                table: "tour_products");

            migrationBuilder.DropColumn(
                name: "agency_id",
                schema: "tour",
                table: "tour_products");
        }
    }
}
