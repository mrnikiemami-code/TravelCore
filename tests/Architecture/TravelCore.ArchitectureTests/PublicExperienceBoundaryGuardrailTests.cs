using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P14-T001 / P14-R1: Public Experience Layer owns Detail/Listing/Landing presentation.
/// Not Search engine. Not Tour catalog. No Booking/Payment.
/// </summary>
public sealed class PublicExperienceBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PublicExperienceContracts_Exist_Without_Persistence_Schema()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.PublicExperience.Contracts");
        Assert.DoesNotContain(Projects, p => p.Name == "TravelCore.Modules.PublicExperience.Infrastructure");
        Assert.DoesNotContain(Projects, p => p.Name == "TravelCore.Modules.PublicExperience.Domain");

        Assert.Equal(
            "PublicExperience",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceOwnershipBoundary.SurfaceOwnerModule);
        Assert.Equal(
            "Tour",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceOwnershipBoundary.CatalogOwnerModule);
        Assert.Equal(
            "Search",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceOwnershipBoundary.SearchOwnerModule);
    }

    [Fact]
    public void PublicExperienceContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.PublicExperience.Contracts");
        Assert.Empty(contracts.ProjectReferences);

        var forbidden = new[] { "Tour", "Seo", "Pricing", "Search", "Booking", "Payment", "AgencyMarketplace" };
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name => forbidden.Any(f => name.Contains($".{f}.", StringComparison.OrdinalIgnoreCase)
                                             || name.EndsWith($".{f}", StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(hits.Count == 0, "PublicExperience.Contracts must not project-reference peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PublicExperience_DoesNotOwn_Booking_SearchEngine_Or_CatalogTypes()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "PublicExperience");
        Assert.True(Directory.Exists(root), root);

        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
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
                        @"\b(class|record|enum|struct|interface)\s+(TourProduct|TourDeparture|Booking|Payment|SearchDocument|IndexPolicy)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Public Experience must not own Tour/Booking/Search/SEO types:\n" + string.Join('\n', hits));

        Assert.True(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
        Assert.True(
            Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Search")),
            "P15 Search exists as Discovery owner; PublicExperience must not own it.");
    }

    [Fact]
    public void Frontend_PublicExperience_Surfaces_Match_Contracts()
    {
        var path = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "surfaces.ts");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("detail", text, StringComparison.Ordinal);
        Assert.Contains("listing", text, StringComparison.Ordinal);
        Assert.Contains("landing", text, StringComparison.Ordinal);
        Assert.Contains("PublicExperience", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", text, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicDetailStickyActions_Are_Not_Booking()
    {
        var path = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "detail-sticky-actions.tsx");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("View departures", text, StringComparison.Ordinal);
        Assert.Contains("View price", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Pay Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Reserve Seat", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Checkout", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/api/booking", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Listing_And_Landing_Routes_Are_Not_Search_Engine()
    {
        var listingContract = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "listing-landing.ts");
        Assert.True(File.Exists(listingContract), listingContract);
        var contract = File.ReadAllText(listingContract);
        Assert.Contains("Discovery", contract, StringComparison.Ordinal);
        Assert.Contains("SearchIntent", contract, StringComparison.Ordinal);
        Assert.Contains("LANDING_IS_FILTERED_LISTING = false", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", contract, StringComparison.Ordinal);
        Assert.DoesNotContain("to_tsvector", contract, StringComparison.Ordinal);

        var listingPage = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "app",
            "[locale]",
            "tours",
            "page.tsx");
        Assert.True(File.Exists(listingPage), listingPage);
        var listing = File.ReadAllText(listingPage);
        Assert.Contains("PublicTourListingView", listing, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", listing, StringComparison.Ordinal);
        Assert.DoesNotContain("to_tsvector", listing, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", listing, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", listing, StringComparison.Ordinal);

        var landingPage = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "app",
            "[locale]",
            "tours",
            "[slug]",
            "[intent]",
            "page.tsx");
        Assert.True(File.Exists(landingPage), landingPage);
        var landing = File.ReadAllText(landingPage);
        Assert.Contains("PublicTourLandingView", landing, StringComparison.Ordinal);
        Assert.Contains("not a filtered listing", landing, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", landing, StringComparison.Ordinal);
        Assert.DoesNotContain("to_tsvector", landing, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", landing, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", landing, StringComparison.Ordinal);
        Assert.DoesNotContain("IndexPolicy", landing, StringComparison.Ordinal);

        var landingView = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "landing-view.tsx");
        Assert.True(File.Exists(landingView), landingView);
        var landingViewText = File.ReadAllText(landingView);
        Assert.Contains("Curated content", landingViewText, StringComparison.Ordinal);
        Assert.Contains("not a filtered listing", landingViewText, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", landingViewText, StringComparison.Ordinal);
    }

    [Fact]
    public void Shared_Detail_Shell_Does_Not_Duplicate_Kind_Pages()
    {
        var web = Path.Combine(RepoRoot, "src", "frontend", "web", "src");
        Assert.False(File.Exists(Path.Combine(web, "features", "tour-detail", "experience-tour-page.tsx")));
        Assert.False(File.Exists(Path.Combine(web, "features", "tour-detail", "package-tour-page.tsx")));

        var experienceSections = Path.Combine(
            web,
            "features",
            "public-experience",
            "experience-detail-sections.tsx");
        Assert.True(File.Exists(experienceSections), experienceSections);
        var text = File.ReadAllText(experienceSections);
        Assert.Contains("ExperienceTourDetailSections", text, StringComparison.Ordinal);
        Assert.Contains("Itinerary", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/booking", text, StringComparison.Ordinal);

        var endpoints = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Endpoints",
            "TourEndpoints.cs");
        var endpointText = File.ReadAllText(endpoints);
        Assert.Contains("experience/presentation", endpointText, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateQuote", endpointText, StringComparison.Ordinal);
    }

    [Fact]
    public void Related_Tours_Have_Replaceable_Query_Not_Ranking()
    {
        var listPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "related-tours-list.tsx");
        Assert.True(File.Exists(listPath), listPath);
        var list = File.ReadAllText(listPath);
        Assert.Contains("Related tours", list, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", list, StringComparison.Ordinal);
        Assert.DoesNotContain("to_tsvector", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", list, StringComparison.Ordinal);

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Tour",
            "TravelCore.Modules.Tour.Infrastructure",
            "Endpoints",
            "TourEndpoints.cs"));
        Assert.Contains("related-published", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("ts_rank", endpoints, StringComparison.Ordinal);
    }

    [Fact]
    public void Related_Content_Is_Composition_Not_Copied_Into_Tour()
    {
        var listPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "related-content-list.tsx");
        Assert.True(File.Exists(listPath), listPath);
        var list = File.ReadAllText(listPath);
        Assert.Contains("Related content", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", list, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", list, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/tour/products/", list, StringComparison.Ordinal);

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Content",
            "TravelCore.Modules.Content.Infrastructure",
            "Endpoints",
            "ContentEndpoints.cs"));
        Assert.Contains("related-published", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("TourProduct", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", endpoints, StringComparison.Ordinal);

        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceRelatedContentBoundary.CopyContentIntoTourAllowed);
        Assert.Equal("Content", TravelCore.Modules.PublicExperience.Contracts.PublicExperienceRelatedContentBoundary.FactOwner);
        Assert.Equal("Seo", TravelCore.Modules.PublicExperience.Contracts.PublicExperienceRelatedContentBoundary.IndexPolicyOwner);
    }

    [Fact]
    public void AgencyOffer_Presentation_Is_Inquiry_Only_Not_Commercial_Flow()
    {
        var listPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "agency-offers-list.tsx");
        Assert.True(File.Exists(listPath), listPath);
        var list = File.ReadAllText(listPath);
        Assert.Contains("Agency information", list, StringComparison.Ordinal);
        Assert.Contains("Request information", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Pay Now", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Checkout", list, StringComparison.Ordinal);
        Assert.DoesNotContain("PriceOverride", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Commission", list, StringComparison.Ordinal);
        Assert.DoesNotContain("recommended agency", list, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("best agency", list, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pg_trgm", list, StringComparison.Ordinal);
        Assert.DoesNotContain("ts_rank", list, StringComparison.Ordinal);

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "AgencyMarketplace",
            "TravelCore.Modules.AgencyMarketplace.Infrastructure",
            "Endpoints",
            "AgencyMarketplacePublicEndpoints.cs"));
        Assert.Contains("related-published", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireAuthorization", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("IndexPolicy", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("CatalogStatus", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("PriceOverride", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("Commission", endpoints, StringComparison.Ordinal);

        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceAgencyOfferBoundary.CommercialFlowAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceAgencyOfferBoundary.AgencyPriceDisplayAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceAgencyOfferBoundary.RankingAllowed);
        Assert.Equal(
            "AgencyMarketplace",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceAgencyOfferBoundary.FactOwner);
        Assert.Equal(
            "Seo",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceAgencyOfferBoundary.IndexPolicyOwner);
    }

    [Fact]
    public void Filter_Presentation_Is_Not_Search_Faceting()
    {
        var filtersPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "listing-filters.tsx");
        Assert.True(File.Exists(filtersPath), filtersPath);
        var filters = File.ReadAllText(filtersPath);
        Assert.Contains("Presentation filters", filters, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", filters, StringComparison.Ordinal);
        Assert.DoesNotContain("to_tsvector", filters, StringComparison.Ordinal);
        Assert.DoesNotContain("Elasticsearch", filters, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", filters, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", filters, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", filters, StringComparison.Ordinal);

        var statePath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "filter-presentation.ts");
        Assert.True(File.Exists(statePath), statePath);
        var state = File.ReadAllText(statePath);
        Assert.Contains("parseListingFilterCriteria", state, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", state, StringComparison.Ordinal);
        Assert.DoesNotContain("ts_rank", state, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", state, StringComparison.Ordinal);

        var listingPage = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "app",
            "[locale]",
            "tours",
            "page.tsx"));
        Assert.Contains("parseListingFilterCriteria", listingPage, StringComparison.Ordinal);
        Assert.Contains("path: \"tours\"", listingPage, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", listingPage, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", listingPage, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", listingPage, StringComparison.Ordinal);

        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceFilterPresentationBoundary.FacetingAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceFilterPresentationBoundary.RankingAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceFilterPresentationBoundary.FullTextSearchAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceFilterPresentationBoundary.FilteredUrlIsSeoLanding);
        Assert.Equal(
            "Search",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceFilterPresentationBoundary.FutureRetrievalOwner);
        Assert.Equal(
            "Seo",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceFilterPresentationBoundary.IndexPolicyOwner);
    }

    [Fact]
    public void Ugc_Presentation_Is_Composition_Not_Fact_Ownership()
    {
        var listPath = Path.Combine(
            RepoRoot,
            "src",
            "frontend",
            "web",
            "src",
            "features",
            "public-experience",
            "ugc-composition-list.tsx");
        Assert.True(File.Exists(listPath), listPath);
        var list = File.ReadAllText(listPath);
        Assert.Contains("Traveler reviews", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", list, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/search", list, StringComparison.Ordinal);
        Assert.DoesNotContain("pg_trgm", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Elasticsearch", list, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", list, StringComparison.Ordinal);
        Assert.DoesNotContain("Like", list, StringComparison.Ordinal);

        var endpoints = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Ugc",
            "TravelCore.Modules.Ugc.Infrastructure",
            "Endpoints",
            "UgcPublicEndpoints.cs"));
        Assert.Contains("/api/ugc/public", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireAuthorization", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("SetIndexPolicy", endpoints, StringComparison.Ordinal);

        Assert.Equal("Ugc", TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.FactOwner);
        Assert.Equal("PublicExperience", TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.PresentationOwner);
        Assert.Equal("Seo", TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.IndexPolicyOwner);
        Assert.Equal("Search", TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.FutureRetrievalOwner);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.CopyUgcIntoCatalogAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.PubliclyEligibleEqualsSeoIndexed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.PubliclyEligibleEqualsAutomaticallySearchIndexed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.IndependentAverageRatingEngineAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.SearchEngineAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.UgcSeoPagesAllowed);
        Assert.False(TravelCore.Modules.PublicExperience.Contracts.PublicExperienceUgcCompositionBoundary.RankingFromUgcAllowed);
        Assert.Contains(
            "UgcComposition",
            TravelCore.Modules.PublicExperience.Contracts.PublicExperienceDetailComposition.SharedSections,
            StringComparison.Ordinal);
    }
}
