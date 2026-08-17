using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Visa.Infrastructure;

/// <summary>
/// Visa-owned DbContext. Owns PostgreSQL schema <c>visa</c>.
/// No VisaDefinition/requirement tables in T001 (P17-R1 scaffolding only).
/// </summary>
public sealed class VisaDbContext : DbContext
{
    public const string SchemaName = "visa";

    public VisaDbContext(DbContextOptions<VisaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VisaDbContext).Assembly);
    }
}
