using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.PublicExperience.Contracts;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P14-T009: Phase boundary evidence — PE is presentation/composition only;
/// P14-R1…R8 RESOLVED; no Booking/Payment/Search/Pricing ownership.
/// </summary>
public sealed class PublicExperiencePhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();

    [Fact]
    public void P14_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P14-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("P14-R1", text, StringComparison.Ordinal);
        Assert.Contains("P14-R8", text, StringComparison.Ordinal);
        Assert.Contains("Filter presentation ≠ Search faceting", text, StringComparison.Ordinal);
        Assert.Contains("TC-P14-GATE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P14-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new product capability", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicExperience_Keeps_All_P14_Boundaries_Resolved()
    {
        Assert.Equal("PublicExperience", PublicExperienceOwnershipBoundary.SurfaceOwnerModule);
        Assert.Equal("Search", PublicExperienceOwnershipBoundary.SearchOwnerModule);
        Assert.Equal("Seo", PublicExperienceOwnershipBoundary.SeoOwnerModule);
        Assert.False(PublicExperienceListingLandingBoundary.LandingIsFilteredListing);
        Assert.False(PublicExperienceRelatedToursBoundary.RecommendationEngineAllowed);
        Assert.False(PublicExperienceRelatedToursBoundary.SearchRankingAllowed);
        Assert.False(PublicExperienceRelatedContentBoundary.CopyContentIntoTourAllowed);
        Assert.False(PublicExperienceRelatedContentBoundary.ContentPublicationOwnsIndexPolicy);
        Assert.False(PublicExperienceAgencyOfferBoundary.CommercialFlowAllowed);
        Assert.False(PublicExperienceAgencyOfferBoundary.AgencyPriceDisplayAllowed);
        Assert.False(PublicExperienceAgencyOfferBoundary.RankingAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.FacetingAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.RankingAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.FullTextSearchAllowed);
        Assert.False(PublicExperienceFilterPresentationBoundary.FilteredUrlIsSeoLanding);
        Assert.False(PublicExperienceFilterPresentationBoundary.FilteredUrlOwnsIndexPolicy);
        Assert.Equal("Search", PublicExperienceFilterPresentationBoundary.FutureRetrievalOwner);
        Assert.Equal("AgencyMarketplace", PublicExperienceAgencyOfferBoundary.FactOwner);
        Assert.Equal("Content", PublicExperienceRelatedContentBoundary.FactOwner);
        Assert.Equal("Tour", PublicExperienceRelatedToursBoundary.FactOwner);
        Assert.Equal("Ugc", PublicExperienceUgcCompositionBoundary.FactOwner);
        Assert.False(PublicExperienceUgcCompositionBoundary.CopyUgcIntoCatalogAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(PublicExperienceUgcCompositionBoundary.SearchEngineAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.UgcSeoPagesAllowed);
        Assert.False(PublicExperienceUgcCompositionBoundary.RankingFromUgcAllowed);
    }

    [Fact]
    public void PublicExperience_DoesNot_Own_Booking_Payment_Or_Search_Module()
    {
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
        Assert.True(
            Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Search")),
            "P15 Search exists as Discovery owner; PublicExperience remains presentation only.");
        Assert.False(Directory.Exists(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "PublicExperience",
            "TravelCore.Modules.PublicExperience.Infrastructure")));
        Assert.False(Directory.Exists(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "PublicExperience",
            "TravelCore.Modules.PublicExperience.Domain")));
    }

    [Fact]
    public void PublicExperience_Frontend_Keeps_Commercial_And_Search_Engines_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "public-experience");
        Assert.True(Directory.Exists(root), root);

        foreach (var path in Directory.EnumerateFiles(root, "*.ts", SearchOption.TopDirectoryOnly)
                     .Concat(Directory.EnumerateFiles(root, "*.tsx", SearchOption.TopDirectoryOnly)))
        {
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Pay Now", text, StringComparison.Ordinal);
            Assert.DoesNotContain("/api/booking", text, StringComparison.Ordinal);
            Assert.DoesNotContain("/api/search", text, StringComparison.Ordinal);
            Assert.DoesNotContain("pg_trgm", text, StringComparison.Ordinal);
            Assert.DoesNotContain("to_tsvector", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Elasticsearch", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SetIndexPolicy", text, StringComparison.Ordinal);
        }
    }
}
