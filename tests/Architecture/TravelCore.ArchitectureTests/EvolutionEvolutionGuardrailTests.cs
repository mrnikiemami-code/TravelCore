using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Evolution;
using Xunit;

namespace TravelCore.ArchitectureTests;

public sealed class EvolutionEvolutionGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void Evolution_Guardrails_Are_Declared()
    {
        Assert.True(EvolutionOwnershipBoundary.EvolutionGuardrailsImplemented);
        Assert.True(EvolutionOperationalBoundary.OperationalBoundaryImplemented);
        Assert.True(EvolutionDeferredScopeBoundary.DeferredScopeBoundaryImplemented);
    }

    [Fact]
    public void Evolution_T008_Locks_Accepted_Boundaries_T002_Through_T007()
    {
        Assert.True(EvolutionFoundationBoundary.SeparateEvolutionFoundationImplemented);
        Assert.True(EvolutionMetricsGateBoundary.MetricsGateBoundaryImplemented);
        Assert.True(EvolutionSearchEngineBoundary.SearchEvolutionBoundaryImplemented);
        Assert.True(EvolutionProviderExpansionBoundary.ProviderExpansionBoundaryImplemented);
        Assert.True(EvolutionPersonalizationBoundary.PersonalizationBoundaryImplemented);
        Assert.True(EvolutionLoyaltyPromotionsBoundary.LoyaltyPromotionsBoundaryImplemented);
    }

    [Fact]
    public void EvolutionAdvancedPricing_And_Mobile_And_Extraction_Boundaries_Are_Declared()
    {
        Assert.True(EvolutionAdvancedPricingBoundary.AdvancedPricingBoundaryImplemented);
        Assert.True(EvolutionMobileExpansionBoundary.MobileExpansionBoundaryImplemented);
        Assert.True(EvolutionModuleExtractionBoundary.ModuleExtractionBoundaryImplemented);
        Assert.Equal("DEFERRED", EvolutionDeferredScopeBoundary.MicroserviceExtraction);
        Assert.Equal("DEFERRED", EvolutionDeferredScopeBoundary.NativeMobileApps);
    }

    [Fact]
    public void Evolution_T008_Forbids_Evolution_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Evolution");
        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(MicroserviceOrchestrator|SearchClusterClient|RecommendationEngine|LoyaltyPointsLedger|PromotionRulesEngine|NativeMobileAppBackend|DynamicPricingOptimizer|ModuleSplitAutomation|EvolutionAdminController)\b",
            RegexOptions.Compiled);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    return !trimmed.StartsWith("//", StringComparison.Ordinal)
                        && !trimmed.StartsWith("///", StringComparison.Ordinal)
                        && forbiddenType.IsMatch(x.line);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "Evolution T008 forbids evolution product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PostP29_Evidence_Records_T008_And_R6_R7_R8()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "Post-P29-implementation-plan.md"));
        Assert.Contains("TC-Post-P29-T008", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R6", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R7", plan, StringComparison.Ordinal);
        Assert.Contains("Post-P29-R8", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionOperationalBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("EvolutionDeferredScopeBoundary", plan, StringComparison.Ordinal);
    }

    [Fact]
    public void PostP29_Evidence_Pack_Locks_T009_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "Post-P29-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);
        string[] required =
        [
            "TC-Post-P29-T009",
            "Post-P29-R1",
            "Post-P29-R8",
            "Metrics before major evolution",
            "Modular Monolith preserved",
            "Evolution != SearchRanking",
            "Microservice extraction DEFERRED",
            "READY_FOR_GATE",
            "TC-Post-P29-GATE",
            "NOT EXECUTED",
        ];
        foreach (var item in required)
        {
            Assert.Contains(item, text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PostP29_Gate_Evidence_Locks_Acceptance_Artifacts()
    {
        var evidence = Path.Combine(RepoRoot, "docs", "plans", "Post-P29-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(evidence), evidence);
        var text = File.ReadAllText(evidence);
        string[] required =
        [
            "TC-Post-P29-GATE",
            "Post-P29 COMPLETE",
            "Post-P29-R1",
            "Post-P29-R8",
            "TC-Post-P29-T009",
            "No new Evolution product capability",
        ];
        foreach (var item in required)
        {
            Assert.Contains(item, text, StringComparison.Ordinal);
        }
    }
}
