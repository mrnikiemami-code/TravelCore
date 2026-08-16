using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// Content→Destination link guardrails (TC-P08-T006 / P08-R5).
/// </summary>
public sealed class ContentDestinationLinkGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ContentDestinationLinks_AreLogicalWithoutCrossSchemaFk()
    {
        var contentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Content");
        var hits = Directory.EnumerateFiles(contentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"HasOne<.*Destination|TravelCore\.Modules\.Destination\.Domain|principalSchema:\s*""destination""")))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Content must not introduce Destination schema FK/navigation:\n" + string.Join('\n', hits));

        var linkPath = Path.Combine(
            contentRoot,
            "TravelCore.Modules.Content.Domain",
            "ContentItemDestination.cs");
        Assert.True(File.Exists(linkPath), linkPath);
        Assert.Contains("public Guid DestinationId", File.ReadAllText(linkPath), StringComparison.Ordinal);
    }
}
