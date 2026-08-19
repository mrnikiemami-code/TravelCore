using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P27-T006 / P27-R3: provider-neutral dispatch contracts without named production adapters or dispatch persistence.
/// </summary>
public sealed class AnalyticsProviderBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void AnalyticsContracts_Expose_Provider_Port_And_Trust_Boundary()
    {
        Assert.NotNull(typeof(IAnalyticsDispatchProvider));
        Assert.NotNull(typeof(IAnalyticsProviderResolver));
        Assert.NotNull(typeof(AnalyticsProviderKey));
        Assert.NotNull(typeof(AnalyticsProviderTrustBoundary));
        Assert.NotNull(typeof(AnalyticsProviderBoundary));
        Assert.True(AnalyticsOwnershipBoundary.ProviderPortImplemented);
        Assert.True(AnalyticsOwnershipBoundary.ProviderAbstractionImplemented);
        Assert.False(AnalyticsOwnershipBoundary.ProviderImplemented);
    }

    [Fact]
    public void AnalyticsProviderTrustBoundary_Keeps_Zero_Provider_Posture()
    {
        Assert.Equal("NONE", AnalyticsProviderTrustBoundary.NamedProviderSelected);
        Assert.Equal("PublisherCall != DispatchSuccess", AnalyticsProviderTrustBoundary.PublisherCallIsNotDispatchSuccess);
        Assert.Equal("ProviderAck != DownstreamCommit", AnalyticsProviderTrustBoundary.ProviderAckIsNotDownstreamCommit);
        Assert.True(AnalyticsProviderTrustBoundary.ProviderPortImplemented);
        Assert.True(AnalyticsProviderTrustBoundary.ZeroProviderPostureValid);
        Assert.False(AnalyticsProviderTrustBoundary.NamedProductionAdapterImplemented);
        Assert.False(AnalyticsProviderTrustBoundary.ProductionProviderRegistered);
    }

    [Fact]
    public void AnalyticsProviderBoundary_Keeps_Adapters_And_Persistence_Out()
    {
        Assert.Equal("Analytics", AnalyticsProviderBoundary.DispatchOwner);
        Assert.True(AnalyticsProviderBoundary.AnalyticsOwnsProviderAbstraction);
        Assert.True(AnalyticsProviderBoundary.ProviderPortImplemented);
        Assert.False(AnalyticsProviderBoundary.NamedProductionAdapterImplemented);
        Assert.False(AnalyticsProviderBoundary.ProviderExecutionPersistenceImplemented);
        Assert.False(AnalyticsProviderBoundary.DispatchStatePersistenceImplemented);
        Assert.False(AnalyticsProviderBoundary.MixpanelClientImplemented);
        Assert.False(AnalyticsProviderBoundary.GoogleAnalyticsClientImplemented);
    }

    [Fact]
    public void AnalyticsProviderCapability_Lists_Planned_Dispatch_Capabilities_Only()
    {
        var names = Enum.GetNames<AnalyticsProviderCapability>()
            .Where(name => name is not "None")
            .ToArray();
        Assert.Equal(2, names.Length);
        Assert.Contains("EventDispatch", names);
        Assert.Contains("BatchExport", names);
    }

    [Fact]
    public void Analytics_T006_Forbids_Named_Adapters_And_Dispatch_Persistence_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Analytics");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(MixpanelProvider|GoogleAnalyticsProvider|AmplitudeProvider|SegmentProvider|IAnalyticsVendorClient|AnalyticsEventStore|EventWarehouse|AnalyticsDispatchRuntime)\b",
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

        Assert.True(hits.Count == 0, "Analytics T006 forbids named adapter/persistence product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Analytics_T006_Has_No_New_Migration_Or_Product_Table_Additions()
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
        Assert.False(AnalyticsProviderBoundary.ProviderExecutionPersistenceImplemented);
    }

    [Fact]
    public void P27_Evidence_Records_T006_And_R3()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P27-implementation-plan.md"));
        Assert.Contains("TC-P27-T006", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R3", plan, StringComparison.Ordinal);
        Assert.Contains("AnalyticsProviderBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
