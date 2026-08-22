using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.CommercialFinance.Domain;

namespace TravelCore.Modules.CommercialFinance.Infrastructure;

/// <summary>
/// Commercial Finance-owned DbContext. Owns PostgreSQL schema <c>commercial_finance</c>.
/// Logical refs to agency_profile_id, agency_offer_id, booking_id, payment_id — no cross-schema FK.
/// </summary>
public sealed class CommercialFinanceDbContext : DbContext
{
    public const string SchemaName = "commercial_finance";

    public CommercialFinanceDbContext(DbContextOptions<CommercialFinanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<CommissionAgreement> CommissionAgreements => Set<CommissionAgreement>();

    public DbSet<AgencyOfferCommissionOverride> AgencyOfferCommissionOverrides =>
        Set<AgencyOfferCommissionOverride>();

    public DbSet<CommercialObligation> CommercialObligations => Set<CommercialObligation>();

    public DbSet<SettlementPeriod> SettlementPeriods => Set<SettlementPeriod>();

    public DbSet<SettlementRecord> SettlementRecords => Set<SettlementRecord>();

    public DbSet<PayoutInstruction> PayoutInstructions => Set<PayoutInstruction>();

    public DbSet<CommercialFinanceEventConsumptionRecord> EventConsumptionRecords =>
        Set<CommercialFinanceEventConsumptionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommercialFinanceDbContext).Assembly);
    }
}
