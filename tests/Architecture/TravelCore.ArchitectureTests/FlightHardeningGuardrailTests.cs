using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P22-T009: phase-boundary evidence for accepted P22-R1 through P22-R8.
/// Hardening only — no new Flight capability and no GATE close.
/// Complements FlightBoundaryGuardrailTests and FlightPublicJourneyGuardrailTests.
/// </summary>
public sealed class FlightHardeningGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void P22_EvidencePack_Exists_And_DoesNotClose_Gate()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P22-T009-hardening-and-evidence-pack.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);

        string[] required =
        [
            "TC-P22-PLAN ACCEPTED",
            "TC-P22-T001 ACCEPTED",
            "TC-P22-T002 ACCEPTED",
            "TC-P22-T003 ACCEPTED",
            "TC-P22-T004 ACCEPTED",
            "TC-P22-T005 ACCEPTED",
            "TC-P22-T006 ACCEPTED",
            "TC-P22-T007 ACCEPTED",
            "TC-P22-T008 ACCEPTED",
            "TC-P22-T009",
            "TC-P22-GATE NOT EXECUTED",
            "P22-R1",
            "P22-R8",
            "Flight != Tour",
            "FlightBooking != Tour Booking",
            "FlightBooking != HotelBooking",
            "Tour Package Flight != live Flight inventory",
            "Payment Succeeded != FlightBooking Confirmed",
            "PNR Confirmed != FlightBooking Confirmed",
            "Ticket Issued != FlightBooking Confirmed",
            "FlightBookingCancelled != RefundSucceeded",
            "X-TravelCore-Flight-Booking-Access-Token",
            "Production Flight Search Source = NONE",
            "Production Flight Availability Source = NONE",
            "Production Flight Offer Source = NONE",
            "Production Flight Reservation Source = NONE",
            "Production Flight Ticketing Source = NONE",
            "Production Flight Cancellation Source = NONE",
            "Named Flight Supplier = NONE",
            "Production Payment Provider = NONE",
            "READY_FOR_P22_GATE",
            "P22 remains IN PROGRESS",
            "no new product capability",
            "Partial Refund remains DEFERRED",
            "MultiCity remains DEFERRED",
            "amendments/rebooking remain DEFERRED",
        ];

        foreach (var invariant in required)
        {
            Assert.Contains(invariant, text, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("NOT_READY_FOR_P22_GATE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TC-P22-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P22 COMPLETE", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P22-R9", text, StringComparison.Ordinal);
    }

    [Fact]
    public void P22_Closed_Lifecycles_And_Targets_Remain_Exact()
    {
        Assert.Equal(
            new[] { FlightBookingStatus.Pending, FlightBookingStatus.Confirmed, FlightBookingStatus.Cancelled },
            Enum.GetValues<FlightBookingStatus>());
        Assert.Equal(
            new[] { FlightTripType.OneWay, FlightTripType.RoundTrip },
            Enum.GetValues<FlightTripType>());
        Assert.Equal(
            new[]
            {
                FlightPassengerCategory.Adult,
                FlightPassengerCategory.Child,
                FlightPassengerCategory.Infant,
            },
            Enum.GetValues<FlightPassengerCategory>());
        Assert.Equal(
            new[]
            {
                FlightSupplierReservationStatus.Pending,
                FlightSupplierReservationStatus.Confirmed,
                FlightSupplierReservationStatus.Expired,
                FlightSupplierReservationStatus.Cancelled,
            },
            Enum.GetValues<FlightSupplierReservationStatus>());
        Assert.Equal(
            new[]
            {
                FlightSupplierReservationAttemptStatus.Created,
                FlightSupplierReservationAttemptStatus.Initiated,
                FlightSupplierReservationAttemptStatus.Confirmed,
                FlightSupplierReservationAttemptStatus.Failed,
            },
            Enum.GetValues<FlightSupplierReservationAttemptStatus>());
        Assert.Equal(
            new[]
            {
                FlightTicketingAttemptStatus.Created,
                FlightTicketingAttemptStatus.Initiated,
                FlightTicketingAttemptStatus.Succeeded,
                FlightTicketingAttemptStatus.Failed,
            },
            Enum.GetValues<FlightTicketingAttemptStatus>());
        Assert.Equal(
            new[]
            {
                FlightTicketStatus.Pending,
                FlightTicketStatus.Issued,
                FlightTicketStatus.Voided,
                FlightTicketStatus.Refunded,
            },
            Enum.GetValues<FlightTicketStatus>());
        Assert.Equal(
            new[]
            {
                FlightBookingCancellationStatus.Requested,
                FlightBookingCancellationStatus.SupplierReversalPending,
                FlightBookingCancellationStatus.RefundPending,
                FlightBookingCancellationStatus.Completed,
            },
            Enum.GetValues<FlightBookingCancellationStatus>());
        Assert.Equal(
            new[]
            {
                FlightSupplierReversalAttemptStatus.Created,
                FlightSupplierReversalAttemptStatus.Initiated,
                FlightSupplierReversalAttemptStatus.Succeeded,
                FlightSupplierReversalAttemptStatus.Failed,
            },
            Enum.GetValues<FlightSupplierReversalAttemptStatus>());
        Assert.Equal(
            new[]
            {
                FlightBookingCancellationFinancialOutcome.FullRefund,
                FlightBookingCancellationFinancialOutcome.NoRefund,
            },
            Enum.GetValues<FlightBookingCancellationFinancialOutcome>());
        Assert.Equal(
            new[] { PaymentTargetKind.TourBooking, PaymentTargetKind.HotelBooking, PaymentTargetKind.FlightBooking },
            Enum.GetValues<PaymentTargetKind>());
        Assert.Equal(
            new[] { PaymentStatus.Pending, PaymentStatus.Succeeded },
            Enum.GetValues<PaymentStatus>());
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.Equal("flight", FlightOwnershipBoundary.SchemaName);
        Assert.Equal("Flight", FlightOwnershipBoundary.OwnerModule);
        Assert.Equal("ReferenceData", FlightItineraryBoundary.AirportAuthority);
        Assert.Equal("ReferenceData", FlightItineraryBoundary.AirlineAuthority);
        Assert.Equal("DEFERRED", FlightItineraryBoundary.MultiCity);
        Assert.Equal("NO", FlightItineraryBoundary.BirthDateStored);
        Assert.Equal("NO", FlightItineraryBoundary.GenderStored);
        Assert.Equal("NO", FlightItineraryBoundary.NationalityStored);
        Assert.Equal("NO", FlightItineraryBoundary.PassportStored);
        Assert.False(FlightOwnershipBoundary.GenericBookingAbstractionImplemented);
        Assert.False(FlightOwnershipBoundary.SeparateFlightBookingModuleImplemented);
        Assert.False(FlightOwnershipBoundary.SharedDbContextImplemented);
        Assert.False(FlightOwnershipBoundary.PeerSchemaForeignKeyImplemented);
        Assert.False(FlightOwnershipBoundary.SupplierSdkImplemented);
        Assert.Equal("NONE", FlightOwnershipBoundary.NamedFlightSupplier);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionSearchSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionAvailabilitySource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionOfferSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionReservationSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionTicketingSource);
        Assert.Equal("NONE", FlightOwnershipBoundary.ProductionCancellationSource);
        Assert.Equal("NONE", FlightBookingCancellationOwnershipBoundary.ProductionPaymentProvider);
        Assert.False(FlightOfferOwnershipBoundary.SilentRepricingImplemented);
        Assert.False(FlightOfferOwnershipBoundary.SmartRoutingImplemented);
        Assert.False(FlightOfferOwnershipBoundary.AutomaticFailoverImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.PublicListingImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.GenericCrudImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.PublicRefundCommandImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.CardCollectionImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.OperationalHttpRouteImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.RawTokenUrlExposureImplemented);
        Assert.False(PublicFlightBookingCompositionBoundary.RawTokenLocalStorageImplemented);
        Assert.Equal(
            "X-TravelCore-Flight-Booking-Access-Token",
            PublicFlightBookingCompositionBoundary.AccessTokenHeader);
        Assert.Equal(
            "FlightBookingId != Access Credential",
            PublicFlightBookingCompositionBoundary.FlightBookingIdIsNotAccessCredential);
        Assert.Equal(
            "PaymentId != Access Credential",
            PublicFlightBookingCompositionBoundary.PaymentIdIsNotAccessCredential);
        Assert.Equal(
            "ReservationLocator != Access Credential",
            PublicFlightBookingCompositionBoundary.ReservationLocatorIsNotAccessCredential);
        Assert.Equal(
            "TicketingDeadline != OfferExpiresAt",
            FlightOfferOwnershipBoundary.TicketingDeadlineIsNotOfferExpiry);
        Assert.Equal("NO", FlightOfferOwnershipBoundary.PricingModuleGeneralized);
        Assert.Null(typeof(FlightBooking).GetMethod("SetStatus"));
        Assert.Null(typeof(FlightBooking).GetMethod("ForceConfirm"));
        Assert.Null(typeof(FlightBooking).GetMethod("ForceTicket"));
        Assert.Null(typeof(FlightBooking).GetMethod("ForceCancel"));
        Assert.Null(typeof(FlightBooking).GetMethod("MarkPaid"));
        Assert.Null(typeof(FlightBooking).GetMethod("MarkRefunded"));
        Assert.Null(typeof(FlightBooking).Assembly.GetType("TravelCore.Modules.Flight.Domain.BookingBase"));
        Assert.Null(typeof(FlightBooking).Assembly.GetType("TravelCore.Modules.Flight.Domain.GenericBookingAggregate"));
        Assert.Null(typeof(FlightBooking).Assembly.GetType("TravelCore.Modules.Flight.Domain.IFlightSupplierGateway"));
        Assert.Null(typeof(FlightContractsAssemblyMarker).Assembly.GetType(
            "TravelCore.Modules.Flight.Contracts.IFlightSupplierGateway"));
        Assert.Null(typeof(FlightBooking).Assembly.GetType("TravelCore.Modules.Flight.Domain.PNR"));
    }

    [Fact]
    public void P22_GateEvidence_Exists_And_Closes_Phase()
    {
        var path = Path.Combine(RepoRoot, "docs", "plans", "P22-GATE-acceptance-evidence.md");
        Assert.True(File.Exists(path), path);
        var text = File.ReadAllText(path);
        Assert.Contains("TC-P22-T001", text, StringComparison.Ordinal);
        Assert.Contains("TC-P22-T009", text, StringComparison.Ordinal);
        Assert.Contains("P22-R1", text, StringComparison.Ordinal);
        Assert.Contains("P22-R8", text, StringComparison.Ordinal);
        Assert.Contains("Flight != Tour", text, StringComparison.Ordinal);
        Assert.Contains("FlightBooking != Tour Booking", text, StringComparison.Ordinal);
        Assert.Contains("FlightBooking != HotelBooking", text, StringComparison.Ordinal);
        Assert.Contains("Payment Succeeded != FlightBooking Confirmed", text, StringComparison.Ordinal);
        Assert.Contains("PNR Confirmed != FlightBooking Confirmed", text, StringComparison.Ordinal);
        Assert.Contains("Ticket Issued != FlightBooking Confirmed", text, StringComparison.Ordinal);
        Assert.Contains("FlightBookingCancelled != RefundSucceeded", text, StringComparison.Ordinal);
        Assert.Contains("X-TravelCore-Flight-Booking-Access-Token", text, StringComparison.Ordinal);
        Assert.Contains("Production Flight Search Source = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Production Flight Availability Source = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Production Flight Offer Source = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Production Flight Reservation Source = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Production Flight Ticketing Source = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Production Flight Cancellation Source = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Named Flight Supplier = NONE", text, StringComparison.Ordinal);
        Assert.Contains("Production Payment Provider = NONE", text, StringComparison.Ordinal);
        Assert.Contains("TC-P22-GATE COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("P22 COMPLETE", text, StringComparison.Ordinal);
        Assert.Contains("no new product capability", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("P23 — Dynamic Package / Flight + Hotel (PLANNED)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Flight_And_Payment_Infrastructure_Remain_Peer_Isolated()
    {
        var flightInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Flight.Infrastructure");
        var paymentInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Payment.Infrastructure");
        var flightNames = flightInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!).ToArray();
        var paymentNames = paymentInfra.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)!).ToArray();

        Assert.DoesNotContain(
            flightNames,
            name => name is "TravelCore.Modules.Payment.Infrastructure"
                or "TravelCore.Modules.Payment.Domain"
                or "TravelCore.Modules.Booking.Infrastructure"
                or "TravelCore.Modules.Booking.Domain"
                or "TravelCore.Modules.HotelBooking.Infrastructure"
                or "TravelCore.Modules.HotelBooking.Domain"
                or "TravelCore.Modules.Tour.Infrastructure"
                or "TravelCore.Modules.Tour.Domain"
                or "TravelCore.Modules.Pricing.Infrastructure"
                or "TravelCore.Modules.Place.Infrastructure"
                or "TravelCore.Modules.ReferenceData.Infrastructure");
        Assert.DoesNotContain(
            paymentNames,
            name => name is "TravelCore.Modules.Flight.Infrastructure"
                or "TravelCore.Modules.Flight.Domain");
        Assert.Contains(flightNames, name => name == "TravelCore.Modules.Payment.Contracts");
        Assert.Contains(paymentNames, name => name == "TravelCore.Modules.Flight.Contracts");
    }

    [Fact]
    public void Flight_Public_Frontend_Is_Private_Honest_And_Cardless()
    {
        var copy = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "flight-booking", "copy.ts");
        var copyText = File.ReadAllText(copy);
        Assert.Contains("searchTitle: \"جستجوی پرواز\"", copyText, StringComparison.Ordinal);
        Assert.Contains("searchTitle: \"Search flights\"", copyText, StringComparison.Ordinal);
        Assert.Contains("searchTitle: \"البحث عن رحلات\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"پرداخت رزرو پرواز\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"Flight booking payment\"", copyText, StringComparison.Ordinal);
        Assert.Contains("payTitle: \"دفع حجز الرحلة\"", copyText, StringComparison.Ordinal);

        var paymentView = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "features", "flight-booking", "payment-view.tsx"));
        Assert.Contains("sessionStorage", paymentView, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", paymentView, StringComparison.Ordinal);
        Assert.Contains("LtrValue", paymentView, StringComparison.Ordinal);
        Assert.Contains("MoneyText", paymentView, StringComparison.Ordinal);
        Assert.Contains("min-h-11", paymentView, StringComparison.Ordinal);
        Assert.Contains("focus-visible:outline", paymentView, StringComparison.Ordinal);

        var types = File.ReadAllText(Path.Combine(
            RepoRoot, "src", "frontend", "web", "src", "features", "flight-booking", "types.ts"));
        Assert.Contains("tc.flight-booking.access.${flightBookingId}", types, StringComparison.Ordinal);
        Assert.DoesNotContain("accessToken=", types, StringComparison.Ordinal);

        var featureRoot = Path.Combine(RepoRoot, "src", "frontend", "web", "src", "features", "flight-booking");
        var cardHits = Directory.EnumerateFiles(featureRoot, "*.ts*", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x =>
                Regex.IsMatch(x.line, @"\b(PAN|CVV|CVC|cardNumber|card_number|creditCard)\b", RegexOptions.IgnoreCase)
                || x.line.Contains("autocomplete=\"cc-", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(cardHits.Count == 0, "Flight frontend must not collect card data:\n" + string.Join('\n', cardHits));

        var localStorageHits = Directory.EnumerateFiles(featureRoot, "*.ts*", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path).Select((line, i) => (path, line, i)))
            .Where(x => x.line.Contains("localStorage", StringComparison.Ordinal))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();
        Assert.True(
            localStorageHits.Count == 0,
            "Flight token must not enter localStorage:\n" + string.Join('\n', localStorageHits));
    }

    [Fact]
    public void Flight_Does_Not_Share_Schema_Sql_Secrets_Or_Distributed_ExactlyOnce()
    {
        var flightRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Flight");
        var paymentRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        var searchRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Search");

        var flightSql = ScanCs(flightRoot, line =>
            line.Contains("schema: \"payment\"", StringComparison.Ordinal)
            || line.Contains("schema: \"booking\"", StringComparison.Ordinal)
            || line.Contains("schema: \"hotel_booking\"", StringComparison.Ordinal)
            || line.Contains("schema: \"tour\"", StringComparison.Ordinal)
            || line.Contains("schema: \"place\"", StringComparison.Ordinal)
            || line.Contains("FROM payment.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN payment.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("FROM booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("FROM hotel_booking.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN hotel_booking.", StringComparison.OrdinalIgnoreCase));
        var paymentSql = ScanCs(paymentRoot, line =>
            line.Contains("schema: \"flight\"", StringComparison.Ordinal)
            || line.Contains("FROM flight.", StringComparison.OrdinalIgnoreCase)
            || line.Contains("JOIN flight.", StringComparison.OrdinalIgnoreCase));
        Assert.True(flightSql.Count == 0, "Flight must not query peer schemas:\n" + string.Join('\n', flightSql));
        Assert.True(paymentSql.Count == 0, "Payment must not query flight schema:\n" + string.Join('\n', paymentSql));

        var secretHits = ScanCs(flightRoot, line =>
            Regex.IsMatch(line, @"sk_live_|sk_test_|merchant[_-]?key|callback[_-]?secret|api[_-]?secret|password\s*=\s*""", RegexOptions.IgnoreCase)
            && !line.TrimStart().StartsWith("//", StringComparison.Ordinal)
            && !line.TrimStart().StartsWith("///", StringComparison.Ordinal));
        Assert.True(secretHits.Count == 0, "Flight must not commit secrets:\n" + string.Join('\n', secretHits));

        var tokenLogHits = ScanCs(flightRoot, line =>
            Regex.IsMatch(line, @"Log(Information|Warning|Error|Debug|Trace).*AccessToken", RegexOptions.IgnoreCase)
            || line.Contains("LogInformation(rawToken", StringComparison.OrdinalIgnoreCase)
            || line.Contains("LogInformation(token", StringComparison.OrdinalIgnoreCase));
        Assert.True(tokenLogHits.Count == 0, "Raw Flight access token must not be logged:\n" + string.Join('\n', tokenLogHits));

        var rawTokenColumnHits = ScanCs(flightRoot, line =>
            Regex.IsMatch(line, @"HasColumnName\(""raw_token""\)|Column<string>\(""raw_token""\)")
            && !line.TrimStart().StartsWith("//", StringComparison.Ordinal));
        Assert.True(
            rawTokenColumnHits.Count == 0,
            "Raw Flight access token must not be persisted:\n" + string.Join('\n', rawTokenColumnHits));

        var hashPersistHits = ScanCs(
            Path.Combine(flightRoot, "TravelCore.Modules.Flight.Infrastructure", "Persistence"),
            line => line.Contains("token_hash", StringComparison.Ordinal));
        Assert.True(hashPersistHits.Count > 0, "Flight token hash/verifier must be persisted.");

        var exactlyOnceHits = ScanCs(flightRoot, line =>
            Regex.IsMatch(line, @"distributed exactly-once|exactly-once delivery", RegexOptions.IgnoreCase)
            && !line.Contains("not", StringComparison.OrdinalIgnoreCase)
            && !line.Contains("NOT", StringComparison.Ordinal));
        Assert.True(
            exactlyOnceHits.Count == 0,
            "Flight must not claim distributed exactly-once:\n" + string.Join('\n', exactlyOnceHits));

        if (Directory.Exists(searchRoot))
        {
            var searchHits = ScanCs(searchRoot, line =>
                line.Contains("FlightBookingPaymentSucceededIntegrationEvent", StringComparison.Ordinal)
                || line.Contains("X-TravelCore-Flight-Booking-Access-Token", StringComparison.Ordinal)
                || line.Contains("FlightBookingAccessCredential", StringComparison.Ordinal));
            Assert.True(searchHits.Count == 0, "Search must not project Flight transactional data:\n" + string.Join('\n', searchHits));
        }

        var sdkHits = Projects
            .Where(p => p.Name.StartsWith("TravelCore.Modules.Flight", StringComparison.Ordinal))
            .SelectMany(p => p.PackageReferences.Select(pkg => $"{p.Name}:{pkg}"))
            .Where(hit =>
                Regex.IsMatch(hit, @"Amadeus|Sabre|Travelport|NDC|Stripe|Braintree|Adyen|Zarinpal", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(sdkHits.Count == 0, "Flight must not add supplier/provider SDKs:\n" + string.Join('\n', sdkHits));
    }

    [Fact]
    public void Flight_Persistence_Uniqueness_Constraints_Remain_Database_Backed()
    {
        var snapshot = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Flight",
            "TravelCore.Modules.Flight.Infrastructure",
            "Migrations",
            "FlightDbContextModelSnapshot.cs"));
        string[] required =
        [
            "ux_flight_offer_snapshots_flight_booking_id",
            "ux_flight_offer_snapshots_source_offer",
            "ux_flight_booking_monetary_snapshots_flight_booking_id",
            "ux_flight_supplier_reservations_flight_booking_id",
            "ux_flight_supplier_reservations_source_ref",
            "ux_flight_supplier_reservation_attempts_one_unresolved",
            "ux_flight_tickets_booking_passenger",
            "ux_flight_tickets_source_ticket_number",
            "ux_flight_ticketing_attempts_one_unresolved",
            "ux_flight_booking_cancellations_flight_booking_id",
            "ux_flight_supplier_reversal_attempts_one_unresolved_reservation",
            "ux_flight_supplier_reversal_attempts_one_unresolved_ticket",
            "ux_flight_booking_access_credentials_token_hash",
            "ux_flight_booking_payment_evidence_payment_id",
            "ux_flight_booking_payment_compensation_evidence_flight_booking_id",
        ];
        foreach (var name in required)
        {
            Assert.Contains(name, snapshot, StringComparison.Ordinal);
        }

        var paymentSnapshot = File.ReadAllText(Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Payment",
            "TravelCore.Modules.Payment.Infrastructure",
            "Migrations",
            "PaymentDbContextModelSnapshot.cs"));
        Assert.Contains("ux_payments_flight_booking_id", paymentSnapshot, StringComparison.Ordinal);
        Assert.Contains("ck_payments_exactly_one_target", paymentSnapshot, StringComparison.Ordinal);
        Assert.Contains("ck_refunds_exactly_one_target", paymentSnapshot, StringComparison.Ordinal);
        Assert.Contains("flight_booking_id IS NOT NULL", paymentSnapshot, StringComparison.Ordinal);
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
