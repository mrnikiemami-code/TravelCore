using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialHotelBookingScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hotel_booking");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema drop is intentionally omitted: HotelBooking objects may appear in later migrations.
        }
    }
}
