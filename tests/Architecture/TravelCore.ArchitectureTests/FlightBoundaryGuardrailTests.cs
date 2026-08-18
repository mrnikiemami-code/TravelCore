using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P22-T001 / P22-R1: independent Flight module and schema flight;
/// FlightBooking owned by Flight but not implemented; Tour transport stays Tour-owned.
/// </summary>
public sealed class FlightBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void FlightProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Flight.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Flight.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Flight.Infrastructure");
        Assert.DoesNotContain(Projects, p => p.Name.Contains("FlightBooking", StringComparison.Ordinal));
        Assert.Equal("flight", TravelCore.Modules.Flight.Infrastructure.FlightDbContext.SchemaName);
        Assert.Equal("flight", FlightOwnershipBoundary.SchemaName);
        Assert.Equal("Flight", FlightOwnershipBoundary.OwnerModule);
        Assert.Equal("Flight", FlightOwnershipBoundary.TransactionAggregateOwner);
    }

    [Fact]
    public void Flight_DoesNot_Own_Peer_Or_T001_Forbidden_Product()
    {
        Assert.Equal("Flight != Tour", FlightOwnershipBoundary.FlightIsNotTour);
        Assert.Equal("FlightBooking != Tour Booking", FlightOwnershipBoundary.FlightBookingIsNotTourBooking);
        Assert.Equal("FlightBooking != HotelBooking", FlightOwnershipBoundary.FlightBookingIsNotHotelBooking);
        Assert.Equal("Tour Package Flight != live Flight inventory", FlightOwnershipBoundary.TourPackageFlightIsNotLiveInventory);
        Assert.Equal("TourDepartureTransportSegment", FlightOwnershipBoundary.TourTransportType);
        Assert.Equal("Tour", FlightOwnershipBoundary.TourTransportOwner);
        Assert.Equal("NONE", FlightOwnershipBoundary.NamedFlightSupplier);
        Assert.False(FlightOwnershipBoundary.OwnsTourPackageTransport);
        Assert.False(FlightOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.False(FlightOwnershipBoundary.SeparateFlightBookingModuleImplemented);
        Assert.False(FlightOwnershipBoundary.SeparateFlightBookingSchemaImplemented);
        Assert.False(FlightOwnershipBoundary.FlightBookingAggregateImplemented);
        Assert.False(FlightOwnershipBoundary.ItineraryModelImplemented);
        Assert.False(FlightOwnershipBoundary.SegmentModelImplemented);
        Assert.False(FlightOwnershipBoundary.PassengerModelImplemented);
        Assert.False(FlightOwnershipBoundary.SearchModelImplemented);
        Assert.False(FlightOwnershipBoundary.AvailabilityModelImplemented);
        Assert.False(FlightOwnershipBoundary.OfferModelImplemented);
        Assert.False(FlightOwnershipBoundary.PnrModelImplemented);
        Assert.False(FlightOwnershipBoundary.TicketModelImplemented);
        Assert.False(FlightOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(FlightOwnershipBoundary.CancellationModelImplemented);
        Assert.False(FlightOwnershipBoundary.PublicApiImplemented);
        Assert.False(FlightOwnershipBoundary.FrontendImplemented);
        Assert.False(FlightOwnershipBoundary.SupplierSdkImplemented);
        Assert.False(FlightOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(FlightOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(FlightOwnershipBoundary.ProductTablesImplemented);
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightBooking"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.BookingBase"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.GenericBookingAggregate"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.Airport"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.Airline"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightItinerary"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightPassenger"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightOffer"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.PNR"));
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightTicket"));
        Assert.NotNull(typeof(TourDepartureTransportSegment));
    }

    [Fact]
    public void FlightInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.Flight.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Flight.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void FlightDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.Flight.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "Flight.Domain peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Tour_DoesNot_Depend_On_Flight()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Tour.Contracts",
                     "TravelCore.Modules.Tour.Domain",
                     "TravelCore.Modules.Tour.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.Flight", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference Flight:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void Host_Registers_FlightModule_With_No_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        Assert.Contains("new FlightModule()", program, StringComparison.Ordinal);
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Flight",
            "TravelCore.Modules.Flight.Infrastructure",
            "FlightModule.cs"));
        Assert.Contains("AddDbContext<FlightDbContext>", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/flight", module, StringComparison.Ordinal);
    }

    [Fact]
    public void Flight_Does_Not_Add_Supplier_Sdk_Or_Payment_Target()
    {
        var sdkHits = Projects
            .Where(p => p.Name.StartsWith("TravelCore.Modules.Flight", StringComparison.Ordinal))
            .SelectMany(p => p.PackageReferences.Select(pkg => $"{p.Name}:{pkg}"))
            .Where(hit => Regex.IsMatch(hit, @"Amadeus|Sabre|Travelport|NDC", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(sdkHits.Count == 0, "Flight must not add supplier SDKs:\n" + string.Join('\n', sdkHits));

        var payment = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Contracts",
            "PaymentTargetKind.cs"));
        Assert.Contains("TourBooking", payment, StringComparison.Ordinal);
        Assert.Contains("HotelBooking", payment, StringComparison.Ordinal);
        Assert.DoesNotContain("Flight", payment, StringComparison.Ordinal);
    }

    [Fact]
    public void Flight_T001_MustNotImplement_Deferred_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Flight");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(FlightBooking|FlightBookingId|FlightBookingStatus|Airport|Airline|Carrier|FlightItinerary|FlightJourney|FlightSegment|FlightLeg|FlightPassenger|FlightSearch|FlightAvailability|FlightOffer|PNR|FlightTicket|BookingBase|Booking<|GenericBookingAggregate|IFlightSearchSource|IFlightAvailabilitySource|IFlightReservationSource|IFlightTicketingSource|IFlightSupplierGateway)\b",
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
            "T001 forbids Flight deferred product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Flight_Source_Keeps_Peer_Sql_Api_And_Frontend_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "Flight");
        var forbidden = new[]
        {
            "principalSchema:",
            "HasOne<",
            "MapGet(\"/api/flight",
            "MapPost(\"/api/flight",
            "Amadeus",
            "Sabre",
            "Travelport",
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

        Assert.True(hits.Count == 0, "Flight T001 must not add peer SQL, APIs, or SDKs:\n" + string.Join('\n', hits));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "flight")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "flights")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "flight-bookings")));
    }

    [Fact]
    public void Flight_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P22-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P22-R1 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P22-R2", text, StringComparison.Ordinal);
        Assert.Contains("P22-R8", text, StringComparison.Ordinal);
        Assert.Contains("schema `flight`", text, StringComparison.Ordinal);
        Assert.Contains("Flight != Tour", text, StringComparison.Ordinal);
        Assert.Contains("FlightBooking != Tour Booking", text, StringComparison.Ordinal);
        Assert.Contains("FlightBooking != HotelBooking", text, StringComparison.Ordinal);
        Assert.Contains("Tour Package Flight != live Flight inventory", text, StringComparison.Ordinal);
        Assert.Contains("TC-P22-GATE", text, StringComparison.Ordinal);
        Assert.Contains("TC-P22-T002 NOT EXECUTED", text, StringComparison.Ordinal);
    }

    private static bool IsGeneratedOrBin(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForbiddenPeer(string name) =>
        name is "TravelCore.Modules.Tour.Infrastructure"
            or "TravelCore.Modules.Tour.Domain"
            or "TravelCore.Modules.Booking.Infrastructure"
            or "TravelCore.Modules.Booking.Domain"
            or "TravelCore.Modules.HotelBooking.Infrastructure"
            or "TravelCore.Modules.HotelBooking.Domain"
            or "TravelCore.Modules.Payment.Infrastructure"
            or "TravelCore.Modules.Payment.Domain"
            or "TravelCore.Modules.Pricing.Infrastructure"
            or "TravelCore.Modules.Pricing.Domain"
            or "TravelCore.Modules.Place.Infrastructure"
            or "TravelCore.Modules.Place.Domain"
            or "TravelCore.Modules.ReferenceData.Infrastructure"
            or "TravelCore.Modules.ReferenceData.Domain"
            or "TravelCore.Modules.Search.Infrastructure";
}
