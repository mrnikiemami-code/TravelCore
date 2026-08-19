using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.B2B.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialB2BScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "b2b");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema drop is intentionally omitted: B2B objects may appear in later migrations.
        }
    }
}
