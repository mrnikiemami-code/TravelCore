using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Visa.Domain;

namespace TravelCore.Modules.Visa.Infrastructure;

/// <summary>
/// Visa-owned DbContext. Owns PostgreSQL schema <c>visa</c>.
/// VisaDefinition + RequirementSet + Applicability + requirement facts + processing/validity/stay/entry + official fees + public reads (TC-P17-T007). No peer-schema FK.
/// </summary>
public sealed class VisaDbContext : DbContext
{
    public const string SchemaName = "visa";

    public DbSet<VisaDefinition> VisaDefinitions => Set<VisaDefinition>();

    public DbSet<VisaRequirementSet> VisaRequirementSets => Set<VisaRequirementSet>();

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
