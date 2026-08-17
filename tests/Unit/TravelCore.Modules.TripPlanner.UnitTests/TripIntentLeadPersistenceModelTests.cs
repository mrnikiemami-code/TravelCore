using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.TripPlanner.Domain;
using TravelCore.Modules.TripPlanner.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Modules.TripPlanner.UnitTests;

/// <summary>
/// Persistence model for TripIntent and Lead (TC-P18-T002). Schema trip_planner only; no peer FK.
/// </summary>
public sealed class TripIntentLeadPersistenceModelTests
{
    [Fact]
    public void TripPlannerModel_Maps_TripIntent_And_Lead_Without_Peer_Fk_Or_Booking_Columns()
    {
        using var db = new TripPlannerDbContext(
            new DbContextOptionsBuilder<TripPlannerDbContext>()
                .UseTravelCorePostgreSql(
                    "Host=127.0.0.1;Database=travelcore_trip_planner_model_probe;Username=x;Password=x",
                    migrationsHistorySchema: TripPlannerDbContext.SchemaName)
                .Options);

        var model = db.Model;
        Assert.Equal("trip_planner", model.GetDefaultSchema());

        var intentType = model.FindEntityType(typeof(TripIntent));
        Assert.NotNull(intentType);
        Assert.Equal("trip_intents", intentType.GetTableName());
        Assert.Equal(TripPlannerDbContext.SchemaName, intentType.GetSchema());
        Assert.Equal("planning_revision", intentType.FindProperty(nameof(TripIntent.PlanningRevision))!.GetColumnName());
        Assert.Equal("planning_note", intentType.FindProperty(nameof(TripIntent.PlanningNote))!.GetColumnName());
        Assert.Null(intentType.FindProperty("DestinationId"));
        Assert.Null(intentType.FindProperty("Budget"));
        Assert.Null(intentType.FindProperty("Email"));

        var leadType = model.FindEntityType(typeof(Lead));
        Assert.NotNull(leadType);
        Assert.Equal("leads", leadType.GetTableName());
        Assert.Equal(TripPlannerDbContext.SchemaName, leadType.GetSchema());
        Assert.Equal(
            "source_trip_intent_id",
            leadType.FindProperty(nameof(Lead.SourceTripIntentId))!.GetColumnName());
        Assert.Null(leadType.FindProperty("BookingId"));
        Assert.Null(leadType.FindProperty("QuoteId"));
        Assert.Null(leadType.FindProperty("OpportunityId"));
        Assert.Null(leadType.FindProperty("Email"));
        Assert.Null(leadType.FindProperty("Phone"));
        Assert.Null(leadType.FindProperty("AgencyId"));

        var leadFk = leadType.GetForeignKeys().Single();
        Assert.Equal(typeof(TripIntent), leadFk.PrincipalEntityType.ClrType);
        Assert.Equal(TripPlannerDbContext.SchemaName, leadFk.PrincipalEntityType.GetSchema());
        Assert.Equal(DeleteBehavior.Restrict, leadFk.DeleteBehavior);

        var snapshotType = model.FindEntityType(typeof(Lead))!
            .FindNavigation(nameof(Lead.Snapshot))!
            .TargetEntityType;
        Assert.Equal("leads", snapshotType.GetTableName());
        Assert.Equal("captured_planning_revision", snapshotType.FindProperty(nameof(LeadSubmissionSnapshot.CapturedPlanningRevision))!.GetColumnName());
        Assert.Equal("captured_planning_note", snapshotType.FindProperty(nameof(LeadSubmissionSnapshot.CapturedPlanningNote))!.GetColumnName());

        Assert.Equal(2, model.GetEntityTypes().Count(e => !e.IsOwned()));
        Assert.DoesNotContain(
            model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()),
            f =>
            {
                var schema = f.PrincipalEntityType.GetSchema();
                return schema is "identity" or "party" or "tour" or "place" or "destination"
                    or "reference_data" or "content" or "media" or "seo" or "search"
                    or "pricing" or "ugc" or "agency_marketplace" or "visa" or "booking";
            });

        var columns = model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Select(p => p.GetColumnName())
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("price", columns);
        Assert.DoesNotContain("quote", columns);
        Assert.DoesNotContain("booking_id", columns);
        Assert.DoesNotContain("party_id", columns);
        Assert.DoesNotContain("email", columns);
        Assert.DoesNotContain("phone", columns);
        Assert.DoesNotContain("agency_id", columns);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
