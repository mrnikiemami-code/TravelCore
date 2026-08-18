using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P13-T001 / P13-R1: Agency Marketplace is an independent module owning schema <c>agency_marketplace</c>.
/// TC-P13-T002 / P13-R2: AgencyProfile is the commercial layer over Party identity (logical PartyId, 0..1).
/// TC-P13-T003 / P13-R3: AgencyOffer is the market listing (logical TourProduct Guid; same-schema AgencyProfile FK).
/// TC-P13-T005 / P13-R5: AgencyOffer does not own capacity — SalesAvailability + optional logical TourDeparture Guid only.
/// TC-P13-T007 / P13-R7: AgencyOffer publication lifecycle is Marketplace-owned — not SEO IndexPolicy and not TourProduct CatalogStatus.
/// </summary>
public sealed class AgencyMarketplaceBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void AgencyMarketplaceProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.AgencyMarketplace.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.AgencyMarketplace.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.AgencyMarketplace.Infrastructure");

        Assert.Equal(
            "agency_marketplace",
            TravelCore.Modules.AgencyMarketplace.Infrastructure.AgencyMarketplaceDbContext.SchemaName);
    }

    [Fact]
    public void AgencyMarketplaceInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.AgencyMarketplace.Infrastructure");
        var violations = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();

        Assert.True(
            violations.Count == 0,
            "AgencyMarketplace.Infrastructure must not project-reference Party/Tour/Pricing/Booking/Payment:\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void AgencyMarketplaceDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.AgencyMarketplace.Domain");
        var forbidden = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "AgencyMarketplace.Domain must stay free of Party/Tour/Pricing/Booking/Payment and peer Infrastructure:\n"
            + string.Join('\n', forbidden));
    }

    [Fact]
    public void AgencyMarketplaceContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.AgencyMarketplace.Contracts");
        var forbidden = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "AgencyMarketplace.Contracts must not project-reference Party/Tour/Pricing/Booking/Payment:\n"
            + string.Join('\n', forbidden));
    }

    [Fact]
    public void AgencyMarketplaceModule_DoesNotOwn_Peer_Or_EarlyProduct_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "AgencyMarketplace");
        Assert.True(Directory.Exists(root), root);

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

                    return Regex.IsMatch(
                               x.line,
                               @"\b(class|record|enum|struct|interface)\s+(TourProduct|TourDeparture|Booking|Payment|PaymentIntent|Reservation|Checkout|Price|Quote|Party|Person|Organization)\b")
                           || Regex.IsMatch(
                               x.line,
                               @"\b(IBookingService|IPaymentService|IPricingService|ICheckoutService|DbSet<\s*(TourProduct|TourDeparture|Booking|Payment))\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Agency Marketplace must not own Party/Tour/Pricing/Booking/Payment types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void AgencyMarketplaceModule_Forbids_PeerSchemaFk_And_SharedDbContext()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "AgencyMarketplace");
        var hits = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""(party|tour|pricing)""|HasOne<.*(Party|Tour|Price)|TravelCore\.Modules\.(Party|Tour|Pricing)\.(Domain|Infrastructure)|(Party|Tour|Pricing)DbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Agency Marketplace must not introduce Party/Tour/Pricing schema FK/nav or share those DbContexts:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_And_Payment_Exist_Independently()
    {
        var booking = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        var payment = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        Assert.True(Directory.Exists(booking), "P19 owns Booking independently; AgencyMarketplace must not absorb it.");
        Assert.True(Directory.Exists(payment), "P20 owns Payment independently; AgencyMarketplace must not absorb it.");
    }

    [Fact]
    public void AgencyMarketplace_Exposes_Logical_Party_Reference_Readiness()
    {
        Assert.True(typeof(TravelCore.Modules.AgencyMarketplace.Domain.MarketplacePartyId).IsValueType);

        Assert.Equal(
            "Party",
            TravelCore.Modules.AgencyMarketplace.Contracts.AgencyPartyIdentityBoundary.IdentitySourceModule);
        Assert.Equal(
            "AgencyMarketplace",
            TravelCore.Modules.AgencyMarketplace.Contracts.AgencyPartyIdentityBoundary.CommercialLayerModule);
    }

    [Fact]
    public void AgencyMarketplaceDomain_Exposes_AgencyProfile_With_Logical_Party_Reference()
    {
        var profileType = typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyProfile);
        Assert.NotNull(profileType.GetProperty("PartyId"));
        Assert.NotNull(profileType.GetProperty("Display"));
        Assert.NotNull(profileType.GetProperty("Contact"));
        Assert.NotNull(profileType.GetProperty("Commercial"));
        Assert.NotNull(profileType.GetProperty("Status"));
        Assert.Equal(
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.MarketplacePartyId),
            profileType.GetProperty("PartyId")!.PropertyType);

        Assert.Null(profileType.GetProperty("Offer"));
        Assert.NotNull(typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer));
        Assert.Equal(
            typeof(Guid),
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer).GetProperty("TourProductId")!.PropertyType);
        Assert.Equal(
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyProfileId),
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer).GetProperty("AgencyProfileId")!.PropertyType);
    }

    [Fact]
    public void AgencyOffer_MustNotOwn_Price_Or_Money_Surface()
    {
        var types = new[]
        {
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer),
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOfferCommercialTerms),
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOfferSalesRules),
            typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOfferSalesAvailability)
        };

        var forbidden = types
            .SelectMany(t => t.GetProperties()
                .Where(p => Regex.IsMatch(
                    p.Name,
                    "Price|Amount|Currency|Discount|Commission|Quote|RateOverride|AvailableSeats|ReservedSeats|^Capacity$|Allocation",
                    RegexOptions.IgnoreCase))
                .Select(p => $"{t.Name}.{p.Name}"))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "AgencyOffer must not own Price/Money surfaces (P13-R4):\n" + string.Join('\n', forbidden));

        var terms = typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOfferCommercialTerms);
        var rules = typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOfferSalesRules);
        Assert.NotNull(terms.GetProperty("Notes"));
        Assert.NotNull(terms.GetProperty("SalesRules"));
        Assert.NotNull(rules.GetProperty("RequiresManualConfirmation"));
        Assert.NotNull(rules.GetProperty("ExclusiveListing"));
        Assert.NotNull(typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer).GetMethod("Deactivate"));
        Assert.NotNull(typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer).GetProperty("SalesAvailability"));
        Assert.NotNull(typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer).GetProperty("ReferencedTourDepartureId"));
        Assert.NotNull(typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer).GetProperty("PublicationStatus"));
        Assert.True(typeof(TravelCore.Modules.AgencyMarketplace.Domain.MarketplaceTourDepartureId).IsValueType);
        Assert.True(typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOfferPublicationStatus).IsEnum);
    }

    [Fact]
    public void AgencyMarketplace_DoesNotOwn_Seo_IndexPolicy_Or_CatalogStatus()
    {
        var offer = typeof(TravelCore.Modules.AgencyMarketplace.Domain.AgencyOffer);
        Assert.Null(offer.GetProperty("IndexPolicy"));
        Assert.Null(offer.GetProperty("CatalogStatus"));
        Assert.Null(offer.GetProperty("Robots"));

        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "AgencyMarketplace");
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

                    return Regex.IsMatch(
                        x.line,
                        @"IndexPolicy|/api/seo|SeoDbContext|CatalogStatus",
                        RegexOptions.IgnoreCase);
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Agency Marketplace must not own SEO IndexPolicy or TourProduct CatalogStatus:\n"
            + string.Join('\n', hits));
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.StartsWith("TravelCore.Modules.Party.", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("TravelCore.Modules.Tour.", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("TravelCore.Modules.Pricing.", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("TravelCore.Modules.Booking.", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("TravelCore.Modules.Payment.", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
