using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Analytics.Contracts;
using TravelCore.Modules.Analytics.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P27-T007 / P27-R4 + P27-R6: event ingestion and publisher interaction boundaries without persistence or runtime consumers.
/// </summary>
public sealed class AnalyticsEventIngestionBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void AnalyticsContracts_Expose_Ingestion_And_Publisher_Ports()
    {
        Assert.NotNull(typeof(IAnalyticsSemanticEventConsumer));
        Assert.NotNull(typeof(AnalyticsSemanticEventEnvelope));
        Assert.NotNull(typeof(AnalyticsIdempotencyBoundary));
        Assert.NotNull(typeof(AnalyticsPublisherInteractionBoundary));
        Assert.NotNull(typeof(AnalyticsEventIngestionBoundary));
        Assert.NotNull(typeof(AnalyticsPrivacyBoundary));
        Assert.True(AnalyticsOwnershipBoundary.IngestionBoundaryImplemented);
    }

    [Fact]
    public void AnalyticsIdempotencyBoundary_Keeps_Downstream_Posture()
    {
        Assert.Equal("FailedDispatch != SourceOfRecordRollback", AnalyticsIdempotencyBoundary.FailedDispatchDoesNotRollbackSourceOfRecord);
        Assert.Equal("NOT ASSUMED", AnalyticsIdempotencyBoundary.ExactlyOnceDispatch);
        Assert.True(AnalyticsIdempotencyBoundary.DownstreamAsyncIngestionPortImplemented);
        Assert.True(AnalyticsIdempotencyBoundary.IdempotentIngestionPostureDeclared);
        Assert.False(AnalyticsIdempotencyBoundary.EventPersistenceImplemented);
        Assert.False(AnalyticsIdempotencyBoundary.OutboxConsumerImplemented);
    }

    [Fact]
    public void AnalyticsPublisherInteractionBoundary_Keeps_Opaque_Reference_Semantics()
    {
        Assert.Equal("Analytics", AnalyticsPublisherInteractionBoundary.DispatchOwner);
        Assert.True(AnalyticsPublisherInteractionBoundary.PublisherInteractionBoundaryImplemented);
        Assert.True(AnalyticsPublisherInteractionBoundary.OpaqueReferenceSemanticsImplemented);
        Assert.False(AnalyticsPublisherInteractionBoundary.PiiPersistenceImplemented);
        Assert.False(AnalyticsPublisherInteractionBoundary.IdentityGraphImplemented);
    }

    [Fact]
    public void AnalyticsPrivacyBoundary_Keeps_Pii_SoR_Out()
    {
        Assert.Equal("Analytics must not become PII SoR", AnalyticsPrivacyBoundary.AnalyticsIsNotPiiSoR);
        Assert.True(AnalyticsPrivacyBoundary.PrivacyBoundaryImplemented);
        Assert.False(AnalyticsPrivacyBoundary.PiiPersistenceImplemented);
        Assert.False(AnalyticsPrivacyBoundary.IdentityGraphImplemented);
    }

    [Fact]
    public void AnalyticsEventIngestionBoundary_Keeps_Async_Consumer_Only()
    {
        Assert.Equal("Analytics", AnalyticsEventIngestionBoundary.DownstreamIngestionOwner);
        Assert.True(AnalyticsEventIngestionBoundary.EventIngestionBoundaryImplemented);
        Assert.True(AnalyticsEventIngestionBoundary.SemanticEventConsumerPortImplemented);
        Assert.False(AnalyticsEventIngestionBoundary.OutboxConsumerImplemented);
        Assert.False(AnalyticsEventIngestionBoundary.SynchronousPublisherToProviderCallImplemented);
        Assert.False(AnalyticsEventIngestionBoundary.ProviderExecutionImplemented);
    }

    [Fact]
    public void Analytics_T007_Forbids_Persistence_And_Runtime_Consumer_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Analytics");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(AnalyticsEventStore|EventWarehouse|OutboxConsumer|AnalyticsOutbox|IdentityGraph|PiiStore|AnalyticsDispatchRuntime)\b",
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

        Assert.True(
            hits.Count == 0,
            "Analytics T007 forbids persistence/runtime consumer product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Analytics_T007_Has_No_New_Migration_Or_Product_Table_Additions()
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
        Assert.False(AnalyticsEventIngestionBoundary.IngestionPersistenceImplemented);
    }

    [Fact]
    public void P27_Evidence_Records_T007_And_R4_R6()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P27-implementation-plan.md"));
        Assert.Contains("TC-P27-T007", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R4", plan, StringComparison.Ordinal);
        Assert.Contains("P27-R6", plan, StringComparison.Ordinal);
        Assert.Contains("AnalyticsEventIngestionBoundary", plan, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }
}
