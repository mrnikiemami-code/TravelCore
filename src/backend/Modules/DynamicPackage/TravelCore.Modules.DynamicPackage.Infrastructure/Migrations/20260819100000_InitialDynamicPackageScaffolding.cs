using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.DynamicPackage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDynamicPackageScaffolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dynamic_package");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema drop is intentionally omitted: DynamicPackage objects may appear in later migrations.
        }
    }
}
