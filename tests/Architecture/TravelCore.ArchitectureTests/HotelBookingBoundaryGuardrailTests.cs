using System.Text.RegularExpressions;
using NodaTime;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P21-T002: HotelBooking owns schema hotel_booking; Place remains catalog authority;
/// stay/rooms/guests exist; no availability/supplier/rate/payment/cancellation yet.
/// </summary>
public sealed class HotelBookingBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void HotelBookingProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.HotelBooking.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.HotelBooking.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.HotelBooking.Infrastructure");
        Assert.Equal("hotel_booking", TravelCore.Modules.HotelBooking.Infrastructure.HotelBookingDbContext.SchemaName);
        Assert.Equal("hotel_booking", HotelBookingOwnershipBoundary.SchemaName);
        Assert.Equal("Place", HotelBookingOwnershipBoundary.CatalogOwner);
        Assert.Equal("PlaceId", HotelBookingOwnershipBoundary.CatalogIdentity);
        Assert.Equal("NONE", HotelBookingOwnershipBoundary.NamedHotelSupplier);
    }

    [Fact]
    public void HotelBooking_DoesNot_Own_Peer_SoT_Or_Product_Types()
    {
        Assert.Equal("HotelBooking", HotelBookingOwnershipBoundary.OwnerModule);
        Assert.Equal("HotelBooking != Place", HotelBookingOwnershipBoundary.HotelBookingIsNotPlace);
        Assert.Equal("HotelBooking != Hotel Catalog", HotelBookingOwnershipBoundary.HotelBookingIsNotHotelCatalog);
        Assert.Equal("HotelBooking != Tour Booking", HotelBookingOwnershipBoundary.HotelBookingIsNotTourBooking);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPlaceCatalog);
        Assert.False(HotelBookingOwnershipBoundary.OwnsTourBooking);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPayment);
        Assert.False(HotelBookingOwnershipBoundary.OwnsPricing);
        Assert.False(HotelBookingOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingAggregateImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingStatusImplemented);
        Assert.True(HotelBookingOwnershipBoundary.RoomModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.GuestModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.AvailabilityHoldModelImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SupplierAdapterImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SupplierSdkImplemented);
        Assert.True(HotelBookingOwnershipBoundary.RateQuoteModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.CancellationModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingApiImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingUiImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(HotelBookingOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(HotelBookingOwnershipBoundary.PlacePersistenceDependencyImplemented);
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBooking"));
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingStatus"));
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.HotelPlaceReference"));
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.RoomReservation"));
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingGuest"));
    }

    [Fact]
    public void HotelBooking_T002_Stay_Rooms_Guests_Keep_R3_Through_R8_Out()
    {
        Assert.Equal("NodaTime.LocalDate", HotelBookingStayBoundary.CheckInType);
        Assert.Equal("NodaTime.LocalDate", HotelBookingStayBoundary.CheckOutType);
        Assert.Equal(typeof(LocalDate), typeof(HotelBooking).GetProperty(nameof(HotelBooking.CheckInDate))!.PropertyType);
        Assert.Equal(typeof(LocalDate), typeof(HotelBooking).GetProperty(nameof(HotelBooking.CheckOutDate))!.PropertyType);
        Assert.Null(typeof(HotelBooking).GetProperty("CheckInTime"));
        Assert.Null(typeof(HotelBooking).GetProperty("CheckOutTime"));
        Assert.Equal(typeof(HotelBookingStatus), typeof(HotelBooking).GetProperty("Status")!.PropertyType);
        Assert.Null(typeof(HotelBookingGuest).GetProperty("BirthDate"));
        Assert.Null(typeof(HotelBookingGuest).GetProperty("Passport"));
        Assert.Null(typeof(RoomReservation).GetProperty("Quantity"));
        Assert.True(HotelBookingStayBoundary.MultiRoomSupported);
        Assert.False(HotelBookingStayBoundary.BirthDateStoredFlag);
        Assert.True(HotelBookingStayBoundary.HotelBookingStatusImplemented);
        Assert.True(HotelBookingStayBoundary.AvailabilityHoldImplemented);
        Assert.True(HotelBookingStayBoundary.SupplierReservationImplemented);
        Assert.False(HotelBookingStayBoundary.RateQuoteImplemented);
        Assert.True(HotelBookingStayBoundary.CancellationImplemented);
        Assert.True(HotelBookingStayBoundary.PaymentIntegrationImplemented);
        Assert.Equal(new[] { "Adult", "Child" }, Enum.GetNames<HotelGuestCategory>());
        Assert.NotEqual(
            typeof(TravelCore.Modules.Booking.Domain.BookingPassenger),
            typeof(HotelBookingGuest));
        Assert.Equal("IHotelAvailabilitySource", HotelAvailabilityOwnershipBoundary.SourcePortName);
        Assert.Equal("NONE", HotelAvailabilityOwnershipBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelAvailabilityOwnershipBoundary.ProductionAvailabilitySource);
        Assert.Equal("Requested, Active, Released, Expired", HotelAvailabilityOwnershipBoundary.HoldStatuses);
        Assert.False(HotelAvailabilityOwnershipBoundary.ProductionFakeSourceImplemented);
        Assert.False(HotelAvailabilityOwnershipBoundary.AutomaticFailoverImplemented);
        Assert.False(HotelAvailabilityOwnershipBoundary.ProcessLocalLockIsAuthority);
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.HotelAvailabilityHold"));
        Assert.NotNull(typeof(HotelBookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingStatus"));
    }

    [Fact]
    public void HotelBookingInfrastructure_MustNotProjectReference_PeerBusinessModules()
    {
        var infra = Projects.Single(p => p.Name == "TravelCore.Modules.HotelBooking.Infrastructure");
        var hits = infra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                IsForbiddenPeerModule(name)
                && !string.Equals(name, "TravelCore.Modules.Payment.Contracts", StringComparison.Ordinal))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "HotelBooking.Infrastructure must not project-reference peer business modules:\n" + string.Join('\n', hits));
        Assert.Contains(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Modules.Payment.Contracts");
        Assert.DoesNotContain(
            infra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Place.Infrastructure"
                or "TravelCore.Modules.Place.Domain"
                or "TravelCore.Modules.Booking.Infrastructure"
                or "TravelCore.Modules.Payment.Infrastructure"
                or "TravelCore.Modules.Payment.Domain"
                or "TravelCore.Modules.Pricing.Infrastructure");
    }

    [Fact]
    public void HotelBookingDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var domain = Projects.Single(p => p.Name == "TravelCore.Modules.HotelBooking.Domain");
        var hits = domain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || IsForbiddenPeerModule(name))
            .ToList();
        Assert.True(
            hits.Count == 0,
            "HotelBooking.Domain must stay free of peer modules:\n" + string.Join('\n', hits));
        Assert.Contains(
            domain.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Identifiers");
        Assert.Contains(
            domain.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name == "TravelCore.Money");
        Assert.DoesNotContain(
            domain.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Place.Domain"
                or "TravelCore.Modules.Booking.Domain"
                or "TravelCore.Modules.Payment.Domain"
                or "TravelCore.Modules.Pricing.Domain");
    }

    [Fact]
    public void HotelBookingContracts_MustNotProjectReference_PeerBusinessModules()
    {
        var contracts = Projects.Single(p => p.Name == "TravelCore.Modules.HotelBooking.Contracts");
        var hits = contracts.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(IsForbiddenPeerModule)
            .ToList();
        Assert.True(
            hits.Count == 0,
            "HotelBooking.Contracts must not project-reference peer business modules:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void HotelBooking_T001_MustNotImplement_Deferred_Product_Types()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "HotelBooking");
        Assert.True(Directory.Exists(root), root);

        var forbiddenType = new Regex(
            @"\b(class|record|enum|struct|interface)\s+(RoomInventory|Allotment|HotelRateOffer(?!Snapshot)|RatePlan|HotelQuote|CancellationPolicy(?!Snapshot)|CancellationPenalty(?!Rule)|CancellationRequest|BookingBase|GenericBookingAggregate|SupplierReservation|SupplierBookingAttempt)\b",
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
            "T002 forbids HotelBooking deferred product types:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void HotelBooking_Module_Keeps_Supplier_Api_And_Ui_Out()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "HotelBooking");
        Assert.True(Directory.Exists(root), root);

        var forbidden = new[]
        {
            "Booking.com",
            "Expedia",
            "Hotelbeds",
            "WebBeds",
            "Amadeus",
            "MapGet(\"/api/hotel",
            "MapPost(\"/api/hotel",
            "MapPut(\"/api/hotel",
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

        Assert.True(
            hits.Count == 0,
            "HotelBooking must not introduce named suppliers or public HotelBooking API/UI:\n" + string.Join('\n', hits));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "hotels", "book")));
        Assert.False(Directory.Exists(Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "hotel-booking")));
    }

    [Fact]
    public void HotelBooking_Csproj_MustNotReference_Supplier_Sdks()
    {
        var root = Path.Combine(RepoRoot, "src", "backend", "Modules", "HotelBooking");
        var forbiddenPackages = new[]
        {
            "Booking.com",
            "Expedia",
            "Hotelbeds",
            "WebBeds",
            "Amadeus",
        };

        var hits = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path);
            foreach (var token in forbiddenPackages)
            {
                if (text.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    hits.Add($"{Path.GetRelativePath(RepoRoot, path)}:{token}");
                }
            }
        }

        Assert.True(hits.Count == 0, "HotelBooking must not package-reference supplier SDKs:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void HotelBooking_Evidence_Keeps_Ascii_Invariants()
    {
        var plan = Path.Combine(RepoRoot, "docs", "plans", "P21-implementation-plan.md");
        Assert.True(File.Exists(plan), plan);
        var text = File.ReadAllText(plan);
        Assert.Contains("P21-R1 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P21-R2 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P21-R3 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P21-R4 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("P21-R5 = RESOLVED", text, StringComparison.Ordinal);
        Assert.Contains("schema `hotel_booking`", text, StringComparison.Ordinal);
        Assert.Contains("Hotel Catalog != Hotel Booking", text, StringComparison.Ordinal);
        Assert.Contains("HotelBooking != Tour Booking", text, StringComparison.Ordinal);
        Assert.Contains("P21-R2", text, StringComparison.Ordinal);
        Assert.Contains("P21-R8", text, StringComparison.Ordinal);
        Assert.Contains("TC-P21-GATE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void HotelBooking_Host_Registers_Module_Without_Endpoints()
    {
        var program = File.ReadAllText(Path.Combine(RepoRoot, "src", "backend", "TravelCore.Api", "Program.cs"));
        Assert.Contains("new HotelBookingModule()", program, StringComparison.Ordinal);
        var module = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "HotelBooking",
            "TravelCore.Modules.HotelBooking.Infrastructure",
            "HotelBookingModule.cs"));
        Assert.Contains("AddDbContext<HotelBookingDbContext>", module, StringComparison.Ordinal);
        Assert.Contains("IHotelAvailabilitySourceResolver", module, StringComparison.Ordinal);
        Assert.DoesNotContain("IHotelAvailabilitySource,", module, StringComparison.Ordinal);
        Assert.Contains("IHotelRateOfferSourceResolver", module, StringComparison.Ordinal);
        Assert.DoesNotContain("IHotelRateOfferSource,", module, StringComparison.Ordinal);
        Assert.Contains("IHotelReservationSourceResolver", module, StringComparison.Ordinal);
        Assert.DoesNotContain("IHotelReservationSource,", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapGet", module, StringComparison.Ordinal);
        Assert.DoesNotContain("MapPost", module, StringComparison.Ordinal);
    }

    [Fact]
    public void HotelBooking_T004_Rate_Offer_Snapshots_Keep_R5_Through_R8_Out()
    {
        var domain = typeof(HotelBookingDomainAssemblyMarker).Assembly;
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelRateOfferSnapshot"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelRateOfferSnapshotId"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelRoomRateSnapshot"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingMonetarySnapshot"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelChargeComponentSnapshot"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelCancellationPolicySnapshot"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelCancellationPenaltyRule"));
        Assert.Null(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelRateOffer"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelBookingStatus"));
        Assert.NotNull(domain.GetType("TravelCore.Modules.HotelBooking.Domain.HotelSupplierReservation"));
        Assert.Null(domain.GetType("TravelCore.Modules.HotelBooking.Domain.SupplierReservation"));
        Assert.Null(domain.GetType("TravelCore.Modules.HotelBooking.Domain.CancellationPolicy"));
        Assert.Null(domain.GetType("TravelCore.Modules.HotelBooking.Domain.CancellationPenalty"));
        Assert.True(HotelBookingOwnershipBoundary.RateQuoteModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.CancellationModelImplemented);
        Assert.True(HotelBookingOwnershipBoundary.HotelBookingStatusImplemented);
        Assert.True(HotelBookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(HotelBookingOwnershipBoundary.HotelBookingApiImplemented);
        Assert.Equal("NONE", HotelRateOfferOwnershipBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelRateOfferOwnershipBoundary.ProductionHotelRateSource);
        Assert.False(HotelRateOfferOwnershipBoundary.ProductionFakeRateSourceImplemented);
        Assert.Equal("IHotelRateOfferSource", HotelRateOfferOwnershipBoundary.SourcePortName);
        Assert.Equal("DEFERRED", HotelRateOfferOwnershipBoundary.P20PartialRefund);
        Assert.False(HotelRateOfferOwnershipBoundary.PartialRefundExecutionImplemented);
        Assert.False(TravelCore.Modules.Payment.Domain.PaymentRefundBoundary.PartialRefundImplemented);
        Assert.Equal("NO", HotelRateOfferOwnershipBoundary.PricingModuleGeneralized);
        Assert.Equal("NONE", HotelReservationOwnershipBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelReservationOwnershipBoundary.ProductionHotelReservationSource);
        Assert.Equal("IHotelReservationSource", HotelReservationOwnershipBoundary.SourcePortName);
        Assert.False(HotelReservationOwnershipBoundary.ProductionFakeReservationSourceImplemented);
        Assert.True(HotelReservationOwnershipBoundary.PaymentRequiredForConfirmation);
        Assert.True(HotelReservationOwnershipBoundary.CancellationExecutionImplemented);
        Assert.False(HotelReservationOwnershipBoundary.PublicReservationApiImplemented);

        var paymentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        var paymentText = string.Join(
            '\n',
            Directory.EnumerateFiles(paymentRoot, "*.cs", SearchOption.AllDirectories)
                .Where(p => !IsGeneratedOrBin(p))
                .Select(File.ReadAllText));
        Assert.Contains("PartialRefundImplemented = false", paymentText, StringComparison.Ordinal);
        Assert.DoesNotContain("PartialRefundStatus", paymentText, StringComparison.Ordinal);

        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        Assert.True(Directory.Exists(pricingRoot), pricingRoot);
        Assert.DoesNotContain(
            Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
                .Where(p => !IsGeneratedOrBin(p))
                .Select(p => Path.GetRelativePath(RepoRoot, p)),
            path => path.Contains("HotelBooking", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsForbiddenPeerModule(string name) =>
        name.Contains(".Place.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Place", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Booking.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Booking", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Payment.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Payment", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Tour.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Tour", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Content.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Content", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".Pricing.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".Pricing", StringComparison.OrdinalIgnoreCase)
        || name.Contains(".AgencyMarketplace.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".AgencyMarketplace", StringComparison.OrdinalIgnoreCase)
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
        || name.Contains(".PublicExperience.", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith(".PublicExperience", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
