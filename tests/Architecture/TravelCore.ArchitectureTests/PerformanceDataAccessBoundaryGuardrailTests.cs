using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Performance;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P28-T005: data access and read optimization boundaries without Dapper product or ORM replacement.
/// </summary>
public sealed class PerformanceDataAccessBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void PerformanceDataAccessBoundary_Is_Declared()
    {
        Assert.True(PerformanceDataAccessBoundary.DataAccessBoundaryImplemented);
        Assert.Equal("Data access remains module-schema owned", PerformanceDataAccessBoundary.ModuleOwnedSchemaDataAccess);
        Assert.Equal(
            "No query optimization without measurement evidence",
            PerformanceDataAccessBoundary.NoQueryOptimizationWithoutMeasurement);
        Assert.Equal("LOCKED", PerformanceFoundationBoundary.ProfileBeforeOptimize);
    }

    [Fact]
    public void PerformanceReadOptimizationBoundary_Preserves_Ef_Ownership_And_Evidence_Rules()
    {
        Assert.True(PerformanceReadOptimizationBoundary.ReadOptimizationBoundaryImplemented);
        Assert.Equal(
            "Dapper only when explicitly justified by evidence",
            PerformanceReadOptimizationBoundary.DapperJustifiedByEvidenceOnly);
        Assert.Equal(
            "EF Core owns writes and migrations",
            PerformanceFoundationBoundary.EfOwnsWritesAndMigrations);
        Assert.Equal(
            "Dapper only for justified read projections",
            PerformanceFoundationBoundary.DapperJustifiedReadsOnly);
        Assert.False(PerformanceReadOptimizationBoundary.DapperImplementationWithoutEvidence);
        Assert.False(PerformanceReadOptimizationBoundary.OrmReplacementImplemented);
    }

    [Fact]
    public void Performance_T005_Forbids_Dapper_QueryTuner_And_Orm_Replacement_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(DapperQueryExecutor|IDapperConnectionFactory|QueryIndexTuner|OrmReplacementAdapter|ReadProjectionRepository|SharedReadDbContext|PerformanceMigration|IndexOptimizationService)\b",
            RegexOptions.Compiled);

        var hits = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                var text = File.ReadAllText(path);
                return pattern.Matches(text).Select(m => $"{path}: {m.Value}");
            })
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Performance T005 forbids data-access optimization product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PerformanceProject_DoesNot_Reference_Dapper_Or_EfCore()
    {
        var csproj = File.ReadAllText(
            Path.Combine(RepoRoot, "src", "backend", "Platform", "Performance", "TravelCore.Performance", "TravelCore.Performance.csproj"));
        Assert.DoesNotContain("Dapper", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("EntityFrameworkCore", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Npgsql", csproj, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void P28_Evidence_Records_T005_And_DataAccess_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P28-implementation-plan.md"));
        Assert.Contains("TC-P28-T005", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceDataAccessBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("PerformanceReadOptimizationBoundary", plan, StringComparison.Ordinal);
    }
}
