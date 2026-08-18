using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P21-T009: phase-boundary evidence for accepted P21-R1 through P21-R8.
/// Hardening only — no new HotelBooking capability and no GATE close.
/// </summary>
public sealed class HotelBookingHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void P21_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P21-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);

        string[] required =
        [
            "TC-P21-PLAN ACCEPTED",
            "TC-P21-T001 ACCEPTED",
            "TC-P21-T002 ACCEPTED",
            "TC-P21-T003 ACCEPTED",
            "TC-P21-T004 ACCEPTED",
            "TC-P21-T005 ACCEPTED",
            "TC-P21-T006 ACCEPTED",
            "TC-P21-T007 ACCEPTED",
            "TC-P21-T008 ACCEPTED",
            "TC-P21-T009",
            "TC-P21-GATE NOT EXECUTED",
            "P21-R1",
            "P21-R8",
            "HotelBooking != Place",
            "HotelBooking != Tour Booking",
            "Hotel Catalog != Hotel Booking",
            "Payment Succeeded != HotelBooking Confirmed",
            "HotelBookingCancelled != RefundSucceeded",
            "X-TravelCore-Hotel-Booking-Access-Token",
            "Production Hotel Availability Source = NONE",
            "Production Hotel Rate Source = NONE",
            "Production Hotel Reservation Source = NONE",
            "Named Hotel Supplier = NONE",
            "Production Payment Provider = NONE",
            "P21 READY FOR GATE: YES",
            "P21 remains IN PROGRESS",
            "no new product capability",
            "Partial Refund remains DEFERRED",
            "PayAtProperty remains DEFERRED",
            "amendments/rebooking remain DEFERRED",
        ];

        foreach (var invariant in required)
        {
            Assert.Contains(invariant, text, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("TC-P21-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P21 COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P21-R9", text, StringComparison.Ordinal);
    }

    [Fact]
    public void P21_Closed_Lifecycles_And_Targets_Remain_Exact()
    {
        Assert.Equal(
            new[] { HotelBookingStatus.Pending, HotelBookingStatus.Confirmed, HotelBookingStatus.Cancelled },
            Enum.GetValues<HotelBookingStatus>());
        Assert.Equal(
            new[]
            {
                HotelAvailabilityHoldStatus.Requested,
                HotelAvailabilityHoldStatus.Active,
                HotelAvailabilityHoldStatus.Released,
                HotelAvailabilityHoldStatus.Expired,
            },
            Enum.GetValues<HotelAvailabilityHoldStatus>());
        Assert.Equal(
            new[]
            {
                HotelSupplierReservationStatus.Pending,
                HotelSupplierReservationStatus.Confirmed,
                HotelSupplierReservationStatus.Cancelled,
            },
            Enum.GetValues<HotelSupplierReservationStatus>());
        Assert.Equal(
            new[]
            {
                HotelSupplierReservationAttemptStatus.Created,
                HotelSupplierReservationAttemptStatus.Initiated,
                HotelSupplierReservationAttemptStatus.Confirmed,
                HotelSupplierReservationAttemptStatus.Failed,
            },
            Enum.GetValues<HotelSupplierReservationAttemptStatus>());
        Assert.Equal(
            new[]
            {
                HotelBookingCancellationStatus.Requested,
                HotelBookingCancellationStatus.SupplierCancellationPending,
                HotelBookingCancellationStatus.RefundPending,
                HotelBookingCancellationStatus.Completed,
            },
            Enum.GetValues<HotelBookingCancellationStatus>());
        Assert.Equal(
            new[]
            {
                HotelSupplierCancellationAttemptStatus.Created,
                HotelSupplierCancellationAttemptStatus.Initiated,
                HotelSupplierCancellationAttemptStatus.Confirmed,
                HotelSupplierCancellationAttemptStatus.Failed,
            },
            Enum.GetValues<HotelSupplierCancellationAttemptStatus>());
        Assert.Equal(
            new[] { PaymentTargetKind.TourBooking, PaymentTargetKind.HotelBooking },
            Enum.GetValues<PaymentTargetKind>());
        Assert.Equal(
            new[] { PaymentStatus.Pending, PaymentStatus.Succeeded },
            Enum.GetValues<PaymentStatus>());
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.Equal("hotel_booking", HotelBookingOwnershipBoundary.SchemaName);
        Assert.Equal("Place", HotelBookingOwnershipBoundary.CatalogOwner);
        Assert.False(HotelBookingOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(HotelBookingOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(HotelBookingOwnershipBoundary.SupplierSdkImplemented);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionAvailabilitySource);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionRateSource);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionReservationSource);
        Assert.Equal("NONE", HotelSourceReadinessBoundary.ProductionPaymentProvider);
        Assert.False(HotelSourceReadinessBoundary.SmartRoutingImplemented);
        Assert.False(HotelSourceReadinessBoundary.AutomaticFailoverImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.GenericCrudImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.PublicRefundCommandImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.CardCollectionImplemented);
        Assert.False(PublicHotelBookingCompositionBoundary.OperationalHttpRouteImplemented);
        Assert.Equal(
            "X-TravelCore-Hotel-Booking-Access-Token",
            PublicHotelBookingCompositionBoundary.AccessTokenHeader);
        Assert.Null(typeof(HotelBooking).GetMethod("Confirm"));
        Assert.Null(typeof(HotelBooking).GetMethod("SetStatus"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceConfirm"));
        Assert.Null(typeof(HotelBooking).GetMethod("ForceCancel"));
        Assert.Null(typeof(HotelBooking).GetMethod("MarkPaid"));
        Assert.Null(typeof(HotelBooking).GetMethod("MarkRefunded"));
        Assert.Null(typeof(HotelBooking).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.BookingBase"));
        Assert.Null(typeof(HotelBooking).Assembly.GetType("TravelCore.Modules.HotelBooking.Domain.GenericBookingAggregate"));
    }

    [Fact]
    public void HotelBooking_And_Payment_Infrastructure_Remain_Peer_Isolated()
    {
        var hotelInfra = Projects.Single(p => p.Name == "TravelCore.Modules.HotelBooking.Infrastructure");
        var paymentInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        var hotelNames = hotelInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!).ToArray();
        var paymentNames = paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!).ToArray();

        Assert.DoesNotContain(
            hotelNames,
            name => name is "TravelCore.Modules.Payment.Infrastructure"
                or "TravelCore.Modules.Payment.Domain"
                or "TravelCore.Modules.Booking.Infrastructure"
                or "TravelCore.Modules.Booking.Domain"
                or "TravelCore.Modules.Place.Infrastructure"
                or "TravelCore.Modules.Place.Domain"
                or "TravelCore.Modules.Pricing.Infrastructure");
        Assert.DoesNotContain(
            paymentNames,
            name => name is "TravelCore.Modules.HotelBooking.Infrastructure"
                or "TravelCore.Modules.HotelBooking.Domain");
        Assert.Contains(hotelNames, name => name == "TravelCore.Modules.Payment.Contracts");
        Assert.Contains(paymentNames, name => name == "TravelCore.Modules.HotelBooking.Contracts");
    }

    [Fact]
    public void HotelBooking_Public_Frontend_Is_Private_Honest_And_Cardless()
    {
        var copy = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "hotel-booking", "copy.ts");
        var copyText = File.ReadAllText(copy);
        Assert.Contains("prepareTitle: \"شروع رزرو هتل\"", copyText, StringComparison.Ordinal);
        Assert.Contains("prepareTitle: \"Prepare hotel booking\"", copyText, StringComparison.Ordinal);
        Assert.Contains("prepareTitle: \"إعداد حجز فندق\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"پرداخت رزرو هتل\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"Hotel booking payment\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"دفع حجز الفندق\"", copyText, StringComparison.Ordinal);

        var paymentView = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "features", "hotel-booking", "payment-view.tsx"));
        Assert.Contains("sessionStorage", paymentView, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", paymentView, StringComparison.Ordinal);
        Assert.Contains("LtrValue", paymentView, StringComparison.Ordinal);
        Assert.Contains("MoneyText", paymentView, StringComparison.Ordinal);
        Assert.Contains("min-h-11", paymentView, StringComparison.Ordinal);
        Assert.Contains("focus-visible:outline", paymentView, StringComparison.Ordinal);

        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "hotel-booking");
        var cardHits = Directory.EnumerateFiles(featureRoot, "*.ts*", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x =>
                Regex.IsMatch(x.line, @"\b(PAN|CVV|CVC|cardNumber|card_number|creditCard)\b", RegexOptions.IgnoreCase)
                || x.line.Contains("autocomplete=\"cc-", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(cardHits.Count == 0, "HotelBooking frontend must not collect card data:\n" + string.Join('\n', cardHits));
    }

    [Fact]
    public void HotelBooking_Does_Not_Share_Schema_Sql_Secrets_Or_Distributed_ExactlyOnce()
    {
        var hotelRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "HotelBooking");
        var paymentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        var searchRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");

        var hotelSql = ScanCs(hotelRoot, line =>
            line.Contains("schema: \"payment\"", StringComparison.Ordinal)
            || line.Contains("schema: \"booking\"", StringComparison.Ordinal)
            || line.Contains("schema: \"place\"", StringComparison.Ordinal)
            || line.Contains("FROM payment.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN payment.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("FROM booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("FROM place.", StringComparison.OrdinalIgnoreCase));
        var paymentSql = ScanCs(paymentRoot, line =>
            line.Contains("schema: \"hotel_booking\"", StringComparison.Ordinal)
            || line.Contains("FROM hotel_booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN hotel_booking.", StringComparison.OrdinalIgnoreCase));
        Assert.True(hotelSql.Count == 0, "HotelBooking must not query peer schemas:\n" + string.Join('\n', hotelSql));
        Assert.True(paymentSql.Count == 0, "Payment must not query hotel_booking schema:\n" + string.Join('\n', paymentSql));

        var secretHits = ScanCs(hotelRoot, line =>
            Regex.IsMatch(line, @"sk_live_|sk_test_|merchant[_-]?key|callback[_-]?secret|api[_-]?secret|password\s*=\s*""", RegexOptions.IgnoreCase)
            && !line.TrimStart().StartsWith("//", StringComparison.Ordinal)
            && !line.TrimStart().StartsWith("///", StringComparison.Ordinal));
        Assert.True(secretHits.Count == 0, "HotelBooking must not commit secrets:\n" + string.Join('\n', secretHits));

        var tokenLogHits = ScanCs(hotelRoot, line =>
            Regex.IsMatch(line, @"Log(Information|Warning|Error|Debug|Trace).*AccessToken", RegexOptions.IgnoreCase)
            || line.Contains("LogInformation(rawToken", StringComparison.OrdinalIgnoreCase)
            || line.Contains("LogInformation(token", StringComparison.OrdinalIgnoreCase));
        Assert.True(tokenLogHits.Count == 0, "Raw HotelBooking access token must not be logged:\n" + string.Join('\n', tokenLogHits));

        var exactlyOnceHits = ScanCs(hotelRoot, line =>
            Regex.IsMatch(line, @"distributed exactly-once|exactly-once delivery", RegexOptions.IgnoreCase)
            && !line.Contains("not", StringComparison.OrdinalIgnoreCase)
            && !line.Contains("NOT", StringComparison.Ordinal));
        Assert.True(
            exactlyOnceHits.Count == 0,
            "HotelBooking must not claim distributed exactly-once:\n" + string.Join('\n', exactlyOnceHits));

        if (Directory.Exists(searchRoot))
        {
            var searchHits = ScanCs(searchRoot, line =>
                line.Contains("HotelBookingPaymentSucceededIntegrationEvent", StringComparison.Ordinal)
                || line.Contains("X-TravelCore-Hotel-Booking-Access-Token", StringComparison.Ordinal)
                || line.Contains("HotelBookingAccessCredential", StringComparison.Ordinal));
            Assert.True(searchHits.Count == 0, "Search must not project HotelBooking transactional data:\n" + string.Join('\n', searchHits));
        }

        var sdkHits = Projects
            .Where(p => p.Name.StartsWith("TravelCore.Modules.HotelBooking", StringComparison.Ordinal))
            .SelectMany(p => p.PackageReferences.Select(pkg => $"{p.Name}:{pkg}"))
            .Where(hit =>
                Regex.IsMatch(hit, @"HotelBeds|Expedia|Booking\.com|Amadeus|Sabre|Stripe|Braintree|Adyen|Zarinpal", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(sdkHits.Count == 0, "HotelBooking must not add supplier/provider SDKs:\n" + string.Join('\n', sdkHits));
    }

    private static List<string> ScanCs(string root, Func<string, bool> predicate) =>
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x => predicate(x.line))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
}
