using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P27-T005 / P27-R2: product event taxonomy boundary without provider dispatch or event persistence.
/// </summary>
public sealed class AnalyticsEventTaxonomyBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void AnalyticsModule_Exposes_EventTaxonomy_Models()
    {
        Assert.NotNull(typeof(AnalyticsProductEventKind));
        Assert.NotNull(typeof(AnalyticsEventTaxonomyBoundary));
        Assert.NotNull(typeof(AnalyticsEventReference));
        Assert.NotNull(typeof(AnalyticsSemanticEventEnvelope));
        Assert.True(AnalyticsOwnershipBoundary.EventTaxonomyBoundaryImplemented);
        Assert.False(AnalyticsOwnershipBoundary.EventPersistenceImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProviderImplemented);
    }

    [Fact]
    public void AnalyticsEventTaxonomyBoundary_Keeps_Dispatch_And_Persistence_Out()
    {
        Assert.Contains("SearchPerformed", AnalyticsEventTaxonomyBoundary.EventTaxonomy, StringComparison.Ordinal);
        Assert.Contains("BookingCompleted", AnalyticsEventTaxonomyBoundary.EventTaxonomy, StringComparison.Ordinal);
        Assert.Equal("Analytics", AnalyticsEventTaxonomyBoundary.TaxonomyOwner);
        Assert.True(AnalyticsEventTaxonomyBoundary.AnalyticsOwnsEventTaxonomy);
        Assert.False(AnalyticsEventTaxonomyBoundary.EventPersistenceImplemented);
        Assert.False(AnalyticsEventTaxonomyBoundary.ProviderDispatchImplemented);
        Assert.False(AnalyticsEventTaxonomyBoundary.MixpanelClientImplemented);
        Assert.False(AnalyticsEventTaxonomyBoundary.AnalyticsOwnsSearchRanking);
    }

    [Fact]
    public void AnalyticsProductEventKind_Lists_Roadmap_Events_Only()
    {
        var names = Enum.GetNames<AnalyticsProductEventKind>();
        Assert.Equal(10, names.Length);
        Assert.Contains("SearchPerformed", names);
        Assert.Contains("SearchResultClicked", names);
        Assert.Contains("SearchNoResults", names);
        Assert.Contains("FilterApplied", names);
        Assert.Contains("TourViewed", names);
        Assert.Contains("HotelViewed", names);
        Assert.Contains("QuoteCreated", names);
        Assert.Contains("BookingStarted", names);
        Assert.Contains("BookingCompleted", names);
    }

    [Fact]
    public void Analytics_T005_Forbids_Provider_And_Event_Persistence_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Analytics");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(MixpanelProvider|GoogleAnalyticsProvider|AmplitudeProvider|SegmentProvider|IAnalyticsVendorClient|AnalyticsEventStore|EventWarehouse|AnalyticsDispatchRuntime|AnalyticsWarehouse)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "Analytics T005 forbids provider/persistence product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Analytics_T005_Has_No_New_Migration_Or_Product_Table_Additions()
    {
        var migrationsDir = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Analytics",
            "TravelCore.Modules.Analytics.Infrastructure",
            "Migrations");
        var migrationFiles = Directory.Exists(migrationsDir)
            ? Directory.GetFiles(migrationsDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase)
                    && !f.Contains("InitialAnalyticsScaffolding", StringComparison.OrdinalIgnoreCase))
                .ToList()
            : [];
        Assert.Empty(migrationFiles);
        Assert.False(AnalyticsEventTaxonomyBoundary.EventPersistenceImplemented);
    }

    [Fact]
    public void P27_Evidence_Records_T005_And_R2()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P27-implementation-plan.md"));
        Assert.Contains("TC-P27-T005", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R2", plan, StringComparison.Ordinal);
        Assert.Contains("AnalyticsEventTaxonomyBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
