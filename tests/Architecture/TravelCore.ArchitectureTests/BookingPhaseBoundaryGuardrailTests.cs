using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P19-T009: Phase boundary evidence — Booking owns transactional Tour reservation facts;
/// P19-R1…R8 RESOLVED; no Payment, Confirm, Search/SEO PII leak, or GATE close.
/// </summary>
public sealed class BookingPhaseBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void P19_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P19-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);

        string[] required =
        [
            "TC-P19-PLAN ACCEPTED",
            "TC-P19-T001 ACCEPTED",
            "TC-P19-T002 ACCEPTED",
            "TC-P19-T003 ACCEPTED",
            "TC-P19-T004 ACCEPTED",
            "TC-P19-T005 ACCEPTED",
            "TC-P19-T006 ACCEPTED",
            "TC-P19-T007 ACCEPTED",
            "TC-P19-T008 ACCEPTED",
            "TC-P19-GATE NOT EXECUTED",
            "P19-R1",
            "P19-R8",
            "Booking != Tour",
            "Booking != TourDeparture",
            "Booking != Pricing",
            "Booking != Payment",
            "Booking != TripPlanner",
            "Booking != VisaApplication",
            "Booking != AgencyMarketplace",
            "Booking != Search",
            "Booking != SEO",
            "Booking != Notification Provider",
            "CapacityDefinition != CapacityConsumption",
            "CapacityHoldStatus != BookingStatus",
            "Pending != CapacityHeld",
            "Consumed != BookingConfirmed",
            "Expired Hold != BookingExpired",
            "PlannerTravelerComposition != BookingPassenger",
            "BookingPassenger != Party Person Master",
            "BookingContactSnapshot != Party",
            "BookingContactSnapshot != Identity Account",
            "Price != Quote",
            "Quote != BookingMonetarySnapshot",
            "BookingMonetarySnapshot != PaymentAmount",
            "PaymentSucceeded != BookingConfirmed",
            "BookingCancelled != PaymentRefunded",
            "AgencyOffer != Booking",
            "AgencyOffer != Quote",
            "PublicExperience != Booking Source of Truth",
            "Public Booking initiation != Booking confirmation",
            "BookingId != Access Credential",
            "BookingStatus != PaymentStatus",
            "Booking PII != Search/SEO data",
            "Payment execution remains DEFERRED",
            "executable payment-driven Booking confirmation remains DEFERRED",
            "Confirmed cancellation/refund remains DEFERRED",
            "public Booking initiation ends in Pending",
            "no new product capability",
        ];

        foreach (var invariant in required)
        {
            Assert.Contains(invariant, text, StringComparison.Ordinal);
        }

        Assert.Contains("TC-P19-T009", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P19-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P19 COMPLETE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void P19_GateEvidence_Exists_And_Closes_Phase()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P19-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("TC-P19-T001", text, StringComparison.Ordinal);
        Assert.Contains("TC-P19-T009", text, StringComparison.Ordinal);
        Assert.Contains("P19-R1", text, StringComparison.Ordinal);
        Assert.Contains("P19-R8", text, StringComparison.Ordinal);
        Assert.Contains("Booking != Tour", text, StringComparison.Ordinal);
        Assert.Contains("PublicExperience != Booking Source of Truth", text, StringComparison.Ordinal);
        Assert.Contains("BookingId != Access Credential", text, StringComparison.Ordinal);
        Assert.Contains("Payment execution remains **DEFERRED**", text, StringComparison.Ordinal);
        Assert.Contains("TC-P19-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("P19 COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new Booking capability", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("P20 — Payment (PLANNED)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_Keeps_All_P19_Boundaries_Resolved()
    {
        Assert.Equal("Booking", BookingOwnershipBoundary.OwnerModule);
        Assert.Equal("booking", BookingOwnershipBoundary.SchemaName);
        Assert.Equal("TourDeparture", BookingOwnershipBoundary.InitialTarget);
        Assert.False(BookingOwnershipBoundary.OwnsTourCatalog);
        Assert.False(BookingOwnershipBoundary.OwnsTourDeparture);
        Assert.False(BookingOwnershipBoundary.OwnsCapacityDefinition);
        Assert.True(BookingOwnershipBoundary.OwnsCapacityConsumption);
        Assert.False(BookingOwnershipBoundary.OwnsPricing);
        Assert.False(BookingOwnershipBoundary.OwnsQuote);
        Assert.False(BookingOwnershipBoundary.OwnsPayment);
        Assert.False(BookingOwnershipBoundary.OwnsSearch);
        Assert.False(BookingOwnershipBoundary.OwnsSeo);
        Assert.False(BookingOwnershipBoundary.OwnsNotificationDelivery);
        Assert.False(BookingOwnershipBoundary.OwnsVisaApplication);
        Assert.False(BookingOwnershipBoundary.OwnsTripPlannerLead);
        Assert.False(BookingOwnershipBoundary.OwnsAgencyMarketplace);
        Assert.True(BookingOwnershipBoundary.PublicBookingSurfaceImplemented);
        Assert.True(BookingOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(BookingLifecycleBoundary.UnrestrictedConfirmationImplemented);
        Assert.False(BookingLifecycleBoundary.ConfirmedToCancelledImplemented);
        Assert.True(BookingOrchestrationBoundary.PaymentDrivenConfirmationImplemented);
        Assert.False(BookingOrchestrationBoundary.FakePaymentImplemented);
        Assert.False(PublicBookingCompositionBoundary.ConfirmEndpointImplemented);
        Assert.False(PublicBookingCompositionBoundary.PaymentEndpointImplemented);
        Assert.False(PublicBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicBookingCompositionBoundary.PublicCancellationImplemented);
        Assert.False(PublicBookingCompositionBoundary.AgencyOriginOnPublicInitiationImplemented);
        Assert.Equal("Expired Hold != BookingExpired", CapacityConsumptionBoundary.ExpiredHoldIsNotBookingExpired);
        Assert.Equal("Booking PII != Search/SEO data", BookingPeopleBoundary.BookingPiiIsNotSearchSeoData);
        Assert.Equal(
            new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Cancelled },
            Enum.GetValues<BookingStatus>());
        Assert.Null(typeof(Booking).GetMethod("Confirm"));
        Assert.True(Directory.Exists(Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment")));
    }

    [Fact]
    public void Booking_Public_Routes_Remain_Private_And_Unambiguous()
    {
        var toursRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "tours");
        Assert.True(File.Exists(Path.Combine(toursRoot, "[slug]", "page.tsx")));
        Assert.True(File.Exists(Path.Combine(toursRoot, "[slug]", "[intent]", "page.tsx")));
        Assert.True(File.Exists(Path.Combine(toursRoot, "[slug]", "book", "page.tsx")));
        Assert.False(Directory.Exists(Path.Combine(toursRoot, "[productKey]")));
        Assert.True(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "plan", "page.tsx")));
        Assert.True(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "page.tsx")));
        Assert.False(File.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "page.tsx")));
        Assert.False(Directory.Exists(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "checkout")));

        var bookText = File.ReadAllText(Path.Combine(toursRoot, "[slug]", "book", "page.tsx"));
        var statusText = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "app", "[locale]", "bookings", "[bookingId]", "page.tsx"));
        Assert.Contains("robots: { index: false, follow: false }", bookText, StringComparison.Ordinal);
        Assert.Contains("robots: { index: false, follow: false }", statusText, StringComparison.Ordinal);

        var copy = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "features", "booking", "copy.ts"));
        Assert.Contains("prepareTitle: \"شروع رزرو موقت\"", copy, StringComparison.Ordinal);
        Assert.Contains("prepareTitle: \"Prepare booking\"", copy, StringComparison.Ordinal);
        Assert.Contains("prepareTitle: \"إعداد حجز مؤقت\"", copy, StringComparison.Ordinal);
        Assert.DoesNotContain("Book Now", copy, StringComparison.Ordinal);
        Assert.DoesNotContain("Booking confirmed", copy, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment completed", copy, StringComparison.Ordinal);

        var form = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "features", "booking", "prepare-form.tsx"));
        Assert.Contains("LtrValue", form, StringComparison.Ordinal);
        Assert.Contains("FieldMessage", form, StringComparison.Ordinal);
        Assert.Contains("tone=\"error\"", form, StringComparison.Ordinal);
        Assert.Contains("min-h-touch", form, StringComparison.Ordinal);
        Assert.Contains("{copy.givenName}", form, StringComparison.Ordinal);
        Assert.Contains("{copy.category}", form, StringComparison.Ordinal);

        var statusView = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "features", "booking", "status-view.tsx"));
        Assert.Contains("MoneyText", statusView, StringComparison.Ordinal);
        Assert.Contains("LtrValue", statusView, StringComparison.Ordinal);
        Assert.Contains("BidiText", statusView, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_And_Search_Do_Not_Share_Pii_Or_Payment_Surfaces()
    {
        var searchRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");
        Assert.True(Directory.Exists(searchRoot), searchRoot);
        var searchHits = Directory.EnumerateFiles(searchRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x =>
                x.line.Contains("BookingPassenger", StringComparison.Ordinal)
                || x.line.Contains("BookingContactSnapshot", StringComparison.Ordinal)
                || x.line.Contains("booking_access_credentials", StringComparison.Ordinal)
                || x.line.Contains("X-TravelCore-Booking-Access-Token", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(searchHits.Count == 0, "Search must not index Booking PII:\n" + string.Join('\n', searchHits));

        var bookingInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Booking.Infrastructure");
        Assert.DoesNotContain(
            bookingInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!),
            name => name is "TravelCore.Modules.Search.Infrastructure"
                or "TravelCore.Modules.Search.Domain"
                or "TravelCore.Modules.Seo.Infrastructure"
                or "TravelCore.Modules.Tour.Infrastructure"
                or "TravelCore.Modules.Pricing.Infrastructure"
                or "TravelCore.Modules.Identity.Infrastructure");
    }
}
