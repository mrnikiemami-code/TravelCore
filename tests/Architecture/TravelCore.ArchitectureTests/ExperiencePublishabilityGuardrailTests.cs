using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P10-T008 / P10-R8: Experience publishability reuses TourCatalogStatus (no dual status).
/// </summary>
public sealed class ExperiencePublishabilityGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void ExperiencePublishability_ReusesTourCatalogStatus_Only()
    {
        Assert.Equal(["Draft", "Published", "Inactive"], Enum.GetNames<TourCatalogStatus>());

        var domain = Path.Combine(RepoRoot, "src", "backend", "Modules", "Tour", "TravelCore.Modules.Tour.Domain");
        Assert.True(File.Exists(Path.Combine(domain, "ExperiencePublishability.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "ExperienceCatalogStatus.cs")));
        Assert.False(File.Exists(Path.Combine(domain, "ExperiencePublicationState.cs")));

        var contracts = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Contracts",
            "ExperienceCatalogContracts.cs"));
        Assert.Contains("IExperienceCatalogService", contracts, StringComparison.Ordinal);
        Assert.Contains("P10-R8", contracts, StringComparison.Ordinal);
        Assert.Contains("bookable", contracts, StringComparison.OrdinalIgnoreCase);
    }
}
