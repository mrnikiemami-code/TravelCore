using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.DynamicPackage.Contracts;
using TravelCore.Modules.DynamicPackage.Domain;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P23-T001 / P23-R1: independent DynamicPackage module and schema dynamic_package;
/// DynamicPackageBooking owned by DynamicPackage but not implemented; Flight/Hotel/Payment execution unchanged.
/// </summary>
public sealed class DynamicPackageBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void DynamicPackageProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.DynamicPackage.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.DynamicPackage.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        Assert.Equal(
            "dynamic_package",
            TravelCore.Modules.DynamicPackage.Infrastructure.DynamicPackageDbContext.SchemaName);
        Assert.Equal("dynamic_package", DynamicPackageOwnershipBoundary.SchemaName);
        Assert.Equal("DynamicPackage", DynamicPackageOwnershipBoundary.OwnerModule);
        Assert.Equal("DynamicPackage", DynamicPackageOwnershipBoundary.TransactionAggregateOwner);
        Assert.Equal("DynamicPackageBooking", DynamicPackageOwnershipBoundary.TransactionAggregateName);
    }

    [Fact]
    public void DynamicPackage_DoesNot_Own_Peer_Or_T001_Forbidden_Product()
    {
        Assert.Equal("DynamicPackage != Tour", DynamicPackageOwnershipBoundary.DynamicPackageIsNotTour);
        Assert.Equal("DynamicPackage != Tour Booking", DynamicPackageOwnershipBoundary.DynamicPackageIsNotTourBooking);
        Assert.Equal("DynamicPackage != Flight", DynamicPackageOwnershipBoundary.DynamicPackageIsNotFlight);
        Assert.Equal("DynamicPackage != HotelBooking", DynamicPackageOwnershipBoundary.DynamicPackageIsNotHotelBooking);
        Assert.Equal(
            "DynamicPackageBooking != FlightBooking",
            DynamicPackageOwnershipBoundary.DynamicPackageBookingIsNotFlightBooking);
        Assert.Equal(
            "DynamicPackageBooking != HotelBooking",
            DynamicPackageOwnershipBoundary.DynamicPackageBookingIsNotHotelBooking);
        Assert.Equal(
            "Tour Package Flight != live Flight inventory",
            DynamicPackageOwnershipBoundary.TourPackageFlightIsNotLiveInventory);
        Assert.Equal("TourDepartureTransportSegment", DynamicPackageOwnershipBoundary.TourTransportType);
        Assert.Equal("Tour", DynamicPackageOwnershipBoundary.TourTransportOwner);
        Assert.Equal("Flight", DynamicPackageOwnershipBoundary.FlightBookingOwner);
        Assert.Equal("HotelBooking", DynamicPackageOwnershipBoundary.HotelBookingOwner);
        Assert.Equal("Payment", DynamicPackageOwnershipBoundary.PaymentExecutionOwner);
        Assert.Equal("NONE", DynamicPackageOwnershipBoundary.ProductionCompositionSource);
        Assert.Equal("NONE", DynamicPackageOwnershipBoundary.ProductionOrchestrationSource);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsTourPackageTransport);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsTourBooking);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsFlightBookingExecution);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsHotelBookingExecution);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsPayment);
        Assert.False(DynamicPackageOwnershipBoundary.OwnsPricing);
        Assert.False(DynamicPackageOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.SeparateDynamicPackageModuleImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.SeparateDynamicPackageSchemaImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.DynamicPackageBookingAggregateImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.CompositionModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PackageOfferModelImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.PackageMonetaryModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.OrchestrationModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.SagaModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.CancellationModelImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PublicApiImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.FrontendImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.SupplierSdkImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.FlightPersistenceDependencyImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.HotelBookingPersistenceDependencyImplemented);
        Assert.False(DynamicPackageOwnershipBoundary.PaymentPersistenceDependencyImplemented);
        Assert.True(DynamicPackageOwnershipBoundary.ProductTablesImplemented);
        Assert.Null(typeof(DynamicPackageDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.DynamicPackage.Domain.DynamicPackageBooking"));
        Assert.Null(typeof(DynamicPackageDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.DynamicPackage.Domain.BookingBase"));
        Assert.Null(typeof(DynamicPackageDomainAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.DynamicPackage.Domain.GenericBookingAggregate"));
        Assert.NotNull(typeof(TourDepartureTransportSegment));
    }

    [Fact]
    public void DynamicPackageInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "DynamicPackage.Infrastructure peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void DynamicPackageDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.DynamicPackage.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeer)
            .ToList();
        Assert.True(hits.Count == 0, "DynamicPackage.Domain peer refs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void Flight_And_HotelBooking_DoNot_Depend_On_DynamicPackage()
    {
        foreach (var name in new[]
                 {
                     "TravelCore.Modules.Flight.Contracts",
                     "TravelCore.Modules.Flight.Domain",
                     "TravelCore.Modules.Flight.Infrastructure",
                     "TravelCore.Modules.HotelBooking.Contracts",
                     "TravelCore.Modules.HotelBooking.Domain",
                     "TravelCore.Modules.HotelBooking.Infrastructure",
                 })
        {
            var project = Projects.Single(p => p.Name == name);
            var hits = project.ProjectReferences
                .Select(r => Path.GetFileNameWithoutExtension(r)!)
                .Where(r => r.StartsWith("TravelCore.Modules.DynamicPackage", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference DynamicPackage:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void Tour_DoesNot_Depend_On_DynamicPackage()
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
                .Where(r => r.StartsWith("TravelCore.Modules.DynamicPackage", StringComparison.Ordinal))
                .ToList();
            Assert.True(hits.Count == 0, $"{name} must not reference DynamicPackage:\n" + string.Join('\n', hits));
        }
    }

    [Fact]
    public void Host_Registers_DynamicPackageModule_After_Flight_With_No_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        var flightIndex = program.IndexOf("new FlightModule()", StringComparison.Ordinal);
        var dynamicPackageIndex = program.IndexOf("new DynamicPackageModule()", StringComparison.Ordinal);
        Assert.True(flightIndex >= 0, "FlightModule must be registered.");
        Assert.True(dynamicPackageIndex > flightIndex, "DynamicPackageModule must register after FlightModule.");
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "DynamicPackage",
            "TravelCore.Modules.DynamicPackage.Infrastructure",
            "DynamicPackageModule.cs"));
        Assert.Contains("AddDbContext<DynamicPackageDbContext>", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
        Assert.DoesNotContain("/api/dynamic-package", module, StringComparison.Ordinal);
    }

    [Fact]
    public void DynamicPackage_Does_Not_Add_Supplier_Sdk_Or_Payment_Target()
    {
        var sdkHits = Projects
            .Where(p => p.Name.StartsWith("TravelCore.Modules.DynamicPackage", StringComparison.Ordinal))
            .SelectMany(p => p.PackageReferences.Select(pkg => $"{p.Name}:{pkg}"))
            .Where(hit => Regex.IsMatch(hit, @"Amadeus|Sabre|Travelport|NDC", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(sdkHits.Count == 0, "DynamicPackage must not add supplier SDKs:\n" + string.Join('\n', sdkHits));

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
        Assert.Contains("FlightBooking", payment, StringComparison.Ordinal);
        Assert.DoesNotContain("DynamicPackage", payment, StringComparison.Ordinal);
    }

    [Fact]
    public void DynamicPackage_T004_MustNotImplement_Deferred_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "DynamicPackage");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(DynamicPackageBooking|DynamicPackageBookingId|DynamicPackageBookingStatus|PackageOffer|PackageSaga|IPackageCompositionSource|IPackageOrchestrationSource|IDynamicPackageSearchSource|BookingBase|Booking<|GenericBookingAggregate)\b",
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
            "T004 forbids DynamicPackage deferred product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void DynamicPackage_Source_Keeps_Peer_Sql_Api_And_Frontend_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "DynamicPackage");
        var forbidden = new[]
        {
            "principalSchema:",
            "HasOne<",
            "MapGet(\"/api/dynamic-package",
            "MapPost(\"/api/dynamic-package",
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

        Assert.True(hits.Count == 0, "DynamicPackage T001 must not add peer SQL, APIs, or SDKs:\n" + string.Join('\n', hits));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "dynamic-package")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "dynamic-packages")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "package-bookings")));
    }

    [Fact]
    public void DynamicPackage_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P23-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P23-R1 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P23-R2", text, StringComparison.Ordinal);
        Assert.Contains("P23-R8", text, StringComparison.Ordinal);
        Assert.Contains("schema `dynamic_package`", text, StringComparison.Ordinal);
        Assert.Contains("DynamicPackage != Tour", text, StringComparison.Ordinal);
        Assert.Contains("DynamicPackageBooking != FlightBooking", text, StringComparison.Ordinal);
        Assert.Contains("DynamicPackageBooking != HotelBooking", text, StringComparison.Ordinal);
        Assert.Contains("TC-P23-GATE", text, StringComparison.Ordinal);
        Assert.Contains("TC-P23-T002 EXECUTED", text, StringComparison.Ordinal);
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
            or "TravelCore.Modules.Tour.Contracts"
            or "TravelCore.Modules.Booking.Infrastructure"
            or "TravelCore.Modules.Booking.Domain"
            or "TravelCore.Modules.Booking.Contracts"
            or "TravelCore.Modules.HotelBooking.Infrastructure"
            or "TravelCore.Modules.HotelBooking.Domain"
            or "TravelCore.Modules.HotelBooking.Contracts"
            or "TravelCore.Modules.Flight.Infrastructure"
            or "TravelCore.Modules.Flight.Domain"
            or "TravelCore.Modules.Flight.Contracts"
            or "TravelCore.Modules.Payment.Infrastructure"
            or "TravelCore.Modules.Payment.Domain"
            or "TravelCore.Modules.Payment.Contracts"
            or "TravelCore.Modules.Pricing.Infrastructure"
            or "TravelCore.Modules.Pricing.Domain"
            or "TravelCore.Modules.Place.Infrastructure"
            or "TravelCore.Modules.Place.Domain"
            or "TravelCore.Modules.ReferenceData.Infrastructure"
            or "TravelCore.Modules.ReferenceData.Domain"
            or "TravelCore.Modules.Search.Infrastructure";
}
