using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelCore.Modules.CommercialFinance.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCommercialFinanceScaffolding : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "commercial_finance");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Schema drop is intentionally omitted: Commercial Finance objects may appear in later migrations.
    }
}
