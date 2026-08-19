using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Hardening;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P29-T006: content sanitization / file security boundary without Media delivery rewrite or AV scanner product.
/// </summary>
public sealed class HardeningFileSecurityBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void HardeningContentSanitizationBoundary_Is_Declared()
    {
        Assert.True(HardeningContentSanitizationBoundary.ContentSanitizationBoundaryImplemented);
        Assert.Equal(
            "Content sanitization is cross-cutting posture",
            HardeningContentSanitizationBoundary.ContentSanitizationIsCrossCuttingPosture);
        Assert.True(HardeningFoundationBoundary.FileSecurityBoundaryImplemented);
    }

    [Fact]
    public void HardeningMediaFileSecurityInteractionBoundary_Preserves_P06_Media_Ownership()
    {
        Assert.True(HardeningMediaFileSecurityInteractionBoundary.MediaFileSecurityInteractionBoundaryImplemented);
        Assert.Equal(
            "Media owns upload validation and delivery",
            HardeningMediaFileSecurityInteractionBoundary.MediaOwnsUploadAndDelivery);
        Assert.Equal(
            "Malware/AV scanning DEFERRED (P06-R7)",
            HardeningMediaFileSecurityInteractionBoundary.MalwareAvScanningDeferred);
        Assert.Equal("Hardening != MediaDelivery", HardeningOwnershipBoundary.HardeningIsNotMediaDelivery);
        Assert.False(HardeningMediaFileSecurityInteractionBoundary.MalwareScannerProductImplemented);
    }

    [Fact]
    public void HardeningModule_DoesNot_Reference_Media()
    {
        var hardening = Projects.Single(p => p.Name == "TravelCore.Hardening");
        var hits = hardening.ProjectReferences
            .Where(r => r.StartsWith("TravelCore.Modules.Media", StringComparison.Ordinal))
            .ToList();
        Assert.True(hits.Count == 0, "Hardening must not reference Media:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Hardening_T006_Forbids_Sanitizer_And_Av_Scanner_Product()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Platform", "Hardening");
        var pattern = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(HtmlSanitizer|ContentSanitizerService|MalwareScanner|AvScanner|MediaDeliveryService|UploadPipelineService|FileSecurityScanner)\b",
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
            "Hardening T006 forbids early sanitizer/AV/delivery product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void P29_Evidence_Records_T006_And_FileSecurity_Boundary()
    {
        var plan = File.ReadAllText(Path.Combine(RepoRoot, "docs", "plans", "P29-implementation-plan.md"));
        Assert.Contains("TC-P29-T006", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningContentSanitizationBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("HardeningMediaFileSecurityInteractionBoundary", plan, StringComparison.Ordinal);
        Assert.Contains("P29-R4", plan, StringComparison.Ordinal);
    }
}
