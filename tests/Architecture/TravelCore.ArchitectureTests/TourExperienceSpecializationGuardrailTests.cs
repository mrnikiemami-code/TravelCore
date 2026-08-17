using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P10-T001: Experience specialization foundation stays under Tour;
/// Package specialty / P11 departure products must not appear.
/// </summary>
public sealed class TourExperienceSpecializationGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ExperienceSpecialization_ExistsUnderTourDomain_WithTourProductIdKey()
    {
        var path = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Domain",
            "TourExperienceSpecialization.cs");
        Assert.True(File.Exists(path), path);

        var text = File.ReadAllText(path);
        Assert.Contains("class TourExperienceSpecialization", text, StringComparison.Ordinal);
        Assert.Contains("TourProductId", text, StringComparison.Ordinal);
        Assert.Contains("TourKind.Experience", text, StringComparison.Ordinal);

        // T001 must not invent itinerary / business policies (ignore doc comments naming deferrals).
        var codeLines = File.ReadAllLines(path)
            .Where(line =>
            {
                var trimmed = line.TrimStart();
                return !(trimmed.StartsWith("//", StringComparison.Ordinal)
                         || trimmed.StartsWith("///", StringComparison.Ordinal)
                         || trimmed.StartsWith("*", StringComparison.Ordinal));
            })
            .ToList();
        var code = string.Join('\n', codeLines);
        Assert.DoesNotContain("Itinerary", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Difficulty", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Eligibility", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Guide", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Meal", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Accommodation", code, StringComparison.Ordinal);
    }

    [Fact]
    public void TourModule_ForbidsPackageSpecialtyAndP11ProductTypes()
    {
        var tourRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour");
        Assert.True(Directory.Exists(tourRoot), tourRoot);

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
                               @"\b(class|record|enum|struct|interface)\s+(TourPackageSpecialization|PackageTourSpecialization|TourDeparture|FlightSegment|TourHotelOption)\b")
                           || Regex.IsMatch(
                               x.line,
                               @"\b(ItineraryDay|class\s+Itinerary|class\s+Stop)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "T001 forbids Package specialty / Itinerary·Day·Stop / P11 product types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void ExperienceSpecialization_PersistsInTourSchema_Only()
    {
        var configPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Persistence",
            "TourExperienceSpecializationConfiguration.cs");
        Assert.True(File.Exists(configPath), configPath);

        var text = File.ReadAllText(configPath);
        Assert.Contains("tour_experience_specializations", text, StringComparison.Ordinal);
        Assert.Contains("TourProductId", text, StringComparison.Ordinal);
        Assert.DoesNotContain("schema(\"package\")", text, StringComparison.OrdinalIgnoreCase);
    }
}
