using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P10-T007 / P10-R4: Experience media = TourProduct Cover/Gallery; Day/Stop media deferred.
/// </summary>
public sealed class ExperienceMediaPostureGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ExperienceMedia_UsesTourProductCoverGallery_Only()
    {
        Assert.Equal(["Cover", "Gallery"], Enum.GetNames<TourMediaRole>());

        var tourDomain = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour", "TravelCore.Modules.Tour.Domain");
        Assert.True(File.Exists(Path.Combine(tourDomain, "TourProductMediaLink.cs")));
        Assert.False(File.Exists(Path.Combine(tourDomain, "ExperienceMediaLink.cs")));
        Assert.False(File.Exists(Path.Combine(tourDomain, "ExperienceItineraryDayMediaLink.cs")));
        Assert.False(File.Exists(Path.Combine(tourDomain, "ExperienceItineraryStopMediaLink.cs")));
    }

    [Fact]
    public void ExperienceMediaContracts_Exist_AndForbidSecondTable()
    {
        var contracts = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Contracts",
            "ExperienceMediaContracts.cs");
        Assert.True(File.Exists(contracts), contracts);
        var text = File.ReadAllText(contracts);
        Assert.Contains("IExperienceMediaService", text, StringComparison.Ordinal);
        Assert.Contains("P10-R4", text, StringComparison.Ordinal);
        Assert.Contains("deferred", text, StringComparison.OrdinalIgnoreCase);

        var migrations = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Migrations");
        var bad = Directory.EnumerateFiles(migrations, "*ExperienceMedia*", SearchOption.TopDirectoryOnly).ToList();
        Assert.True(bad.Count == 0, "Must not invent a second Experience media table:\n" + string.Join('\n', bad));
    }

    [Fact]
    public void TourModule_ForbidsItineraryDayStopMediaTypes()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        var hits = Directory.EnumerateFiles(tourRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
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

                    return Regex.IsMatch(
                        x.line,
                        @"\b(class|record|enum|struct|interface)\s+(ExperienceItineraryDayMedia|ExperienceItineraryStopMedia|ExperienceDayMedia|ExperienceStopMedia)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "Day/Stop media types are deferred:\n" + string.Join('\n', hits));
    }
}
