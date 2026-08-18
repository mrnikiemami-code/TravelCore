using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPaymentScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema drop is intentionally omitted: Payment objects may appear in later migrations.
        }
    }
}
