using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.TripPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTripPlannerScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trip_planner");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema drop is intentionally omitted: TripPlanner objects may appear in later migrations.
        }
    }
}
