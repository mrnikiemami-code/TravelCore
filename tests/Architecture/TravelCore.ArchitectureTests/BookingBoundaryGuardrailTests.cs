using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P19-T001: Booking owns schema booking; no aggregate/lifecycle/passengers/payment yet.
/// </summary>
public sealed class BookingBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void BookingProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Booking.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Booking.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Booking.Infrastructure");
        Assert.Equal("booking", TravelCore.Modules.Booking.Infrastructure.BookingDbContext.SchemaName);
        Assert.Equal("booking", BookingOwnershipBoundary.SchemaName);
        Assert.Equal("TourDeparture", BookingOwnershipBoundary.InitialTarget);
    }

    [Fact]
    public void Booking_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("Booking", BookingOwnershipBoundary.OwnerModule);
        Assert.False(BookingOwnershipBoundary.OwnsTourCatalog);
        Assert.False(BookingOwnershipBoundary.OwnsTourDeparture);
        Assert.False(BookingOwnershipBoundary.OwnsCapacityDefinition);
        Assert.True(BookingOwnershipBoundary.OwnsCapacityConsumption);
        Assert.False(BookingOwnershipBoundary.CapacityConsumptionImplemented);
        Assert.False(BookingOwnershipBoundary.OwnsPricing);
        Assert.False(BookingOwnershipBoundary.OwnsQuote);
        Assert.False(BookingOwnershipBoundary.OwnsPayment);
        Assert.False(BookingOwnershipBoundary.OwnsPartyOrIdentity);
        Assert.False(BookingOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.False(BookingOwnershipBoundary.OwnsSearch);
        Assert.False(BookingOwnershipBoundary.OwnsSeo);
        Assert.False(BookingOwnershipBoundary.OwnsNotificationDelivery);
        Assert.False(BookingOwnershipBoundary.OwnsVisaApplication);
        Assert.False(BookingOwnershipBoundary.OwnsTripPlannerLead);
        Assert.False(BookingOwnershipBoundary.BookingAggregateImplemented);
        Assert.False(BookingOwnershipBoundary.BookingStatusImplemented);
        Assert.False(BookingOwnershipBoundary.CapacityHoldImplemented);
        Assert.False(BookingOwnershipBoundary.BookingPassengerImplemented);
        Assert.False(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.False(BookingOwnershipBoundary.SearchEngineImplemented);
        Assert.False(BookingOwnershipBoundary.AiInfrastructureImplemented);
        Assert.False(BookingOwnershipBoundary.GenericWorkflowEngineImplemented);
        Assert.False(BookingOwnershipBoundary.NotificationProviderImplemented);
    }

    [Fact]
    public void BookingInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void BookingDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void BookingContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "Booking.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_T001_MustNotImplement_Deferred_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(BookingStatus|CapacityHold|SeatHold|Reservation|ReservedSeats|ConfirmedSeats|ReleasedSeats|BookingPassenger|BookingContactSnapshot|PaymentIntent|PaymentStatus|Quote|Price|Checkout|Lead|VisaApplication|AgencyBooking|SearchIndex|RuleEngine|PolicyEngine|WorkflowEngine)\b",
            RegexOptions.Compiled);

        var hits = new List<string>();
        hits.AddRange(
            Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
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

                        return forbiddenType.IsMatch(x.line);
                    }))
                .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}"));

        Assert.True(
            hits.Count == 0,
            "T001 forbids Booking aggregate/lifecycle/hold/passenger/payment product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Booking_Module_Keeps_Search_Ai_Payment_And_Public_Surfaces_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        Assert.True(Directory.Exists(root), root);

        var forbidden = new[]
        {
            "Elasticsearch",
            "OpenSearch",
            "pg_trgm",
            "to_tsvector",
            "embeddings",
            "vector search",
            "RAG",
            "RuleEngine",
            "WorkflowEngine",
            "SmtpClient",
            "Twilio",
            "MapGet(\"/api/booking",
            "MapPost(\"/api/booking",
        };

        var hits = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (IsGeneratedOrBin(path)
                || path.EndsWith("Boundary.cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = File.ReadAllText(path);
            foreach (var token in forbidden)
            {
                if (text.Contains(token, StringComparison.Ordinal))
                {
                    hits.Add($"{Path.GetRelativePath(RepoRoot, path)}:{token}");
                }
            }
        }

        Assert.True(hits.Count == 0, "Booking T001 must not introduce Search/AI/public API:\n" + string.Join('\n', hits));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Booking"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingStatus"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.BookingPassenger"));
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.CapacityHold"));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "checkout")));
        Assert.False(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "page.tsx")));
    }

    [Fact]
    public void Booking_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P19-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("Booking != TourProduct", text, StringComparison.Ordinal);
        Assert.Contains("Booking != TourDeparture", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Price", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Quote", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Payment", text, StringComparison.Ordinal);
        Assert.Contains("P19-R1", text, StringComparison.Ordinal);
        Assert.Contains("schema `booking`", text, StringComparison.Ordinal);
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.Contains(".Tour.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Tour", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Content.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Content", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Pricing.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Pricing", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".AgencyMarketplace.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".AgencyMarketplace", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Place.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Place", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Destination.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Destination", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".ReferenceData.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".ReferenceData", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Media.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Media", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Seo.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Seo", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Search.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Search", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Ugc.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Ugc", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Visa.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Visa", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Identity.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Identity", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Party.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Party", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".TripPlanner.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".TripPlanner", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".PublicExperience.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".PublicExperience", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
