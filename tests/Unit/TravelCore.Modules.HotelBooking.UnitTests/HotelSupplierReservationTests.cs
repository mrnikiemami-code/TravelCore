using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelSupplierReservationTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly HotelPlaceReference Place =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));
    private static readonly LocalDate CheckIn = new(2026, 8, 20);
    private static readonly LocalDate CheckOut = new(2026, 8, 22);

    private static Stay TwoRoomBooking() =>
        Stay.Create(
            Place,
            CheckIn,
            CheckOut,
            HotelBookingContactSnapshot.Create(email: "lead@example.com"),
            [
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, true),
                ]),
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Alan", "Turing", HotelGuestCategory.Adult, false),
                ]),
            ]);

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private static HotelRateOfferSnapshot AcceptOffer(Stay booking, decimal total = 1_000_000m)
    {
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        return HotelRateOfferSnapshot.Accept(
            booking,
            T0,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            "offer-1",
            T0.Minus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            Irr(total),
            [
                new HotelRoomRateLine(rooms[0].Id, Irr(400_000m), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, Irr(600_000m), "sel-2", "rate-2", "BB"),
            ],
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(0m)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, Irr(total)),
            ]);
    }

    [Fact]
    public void New_HotelBooking_Starts_Pending_Without_Payment_Prerequisite()
    {
        var booking = TwoRoomBooking();
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Null(booking.ConfirmedAt);
        Assert.Null(typeof(Stay).GetProperty("PaymentId"));
        Assert.Null(typeof(Stay).GetMethod("Confirm"));
        Assert.Null(typeof(Stay).GetMethod("SetConfirmed"));
        Assert.Null(typeof(Stay).GetMethod("SetStatus"));
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("Failed", Enum.GetNames<HotelBookingStatus>());
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelSupplierReservationStatus>());
        Assert.DoesNotContain("Failed", Enum.GetNames<HotelSupplierReservationStatus>());
        Assert.Equal(
            new[] { "Created", "Initiated", "Confirmed", "Failed" },
            Enum.GetNames<HotelSupplierReservationAttemptStatus>());
    }

    [Fact]
    public void Authoritative_Complete_Reservation_Does_Not_Confirm_Without_Payment_Evidence()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            "CONF-1",
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativeSupplierReservation(
                reservation,
                T0.Plus(Duration.FromMinutes(1)),
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(),
                snapshot.Monetary.Total,
                cancellationTermsMatch: true,
                snapshot.Monetary,
                []));

        Assert.Contains("Payment", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Equal(HotelSupplierReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(HotelSupplierReservationAttemptStatus.Confirmed, attempt.Status);
        Assert.Null(booking.ConfirmedAt);
    }

    [Fact]
    public void Dual_Payment_And_Supplier_Evidence_Confirms_HotelBooking()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            "CONF-1",
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());
        var evidence = HotelBookingPaymentEvidence.Record(
            booking.Id,
            Guid.CreateVersion7(),
            snapshot.Monetary.Total.Amount,
            snapshot.Monetary.CurrencyCode.Value,
            T0.Plus(Duration.FromMinutes(1)));

        booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation,
            evidence,
            T0.Plus(Duration.FromMinutes(1)),
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(),
            snapshot.Monetary.Total,
            cancellationTermsMatch: true,
            snapshot.Monetary,
            []);

        Assert.Equal(HotelBookingStatus.Confirmed, booking.Status);
        Assert.Equal(T0.Plus(Duration.FromMinutes(1)), booking.ConfirmedAt);
        booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation,
            evidence,
            T0.Plus(Duration.FromMinutes(2)),
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(),
            snapshot.Monetary.Total,
            true,
            snapshot.Monetary,
            []);
        Assert.Equal(T0.Plus(Duration.FromMinutes(1)), booking.ConfirmedAt);
    }

    [Fact]
    public void Network_Timeout_Leaves_Attempt_Initiated_Not_Failed()
    {
        var reservation = HotelSupplierReservation.StartPending(HotelBookingId.New(), "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.MarkAttemptInitiated(attempt.Id, T0.Plus(Duration.FromSeconds(30)));

        Assert.Equal(HotelSupplierReservationAttemptStatus.Initiated, attempt.Status);
        Assert.NotEqual(HotelSupplierReservationAttemptStatus.Failed, attempt.Status);
        Assert.True(attempt.IsUnresolved);
        Assert.Equal(HotelSupplierReservationStatus.Pending, reservation.Status);
        Assert.True(HotelReservationOwnershipBoundary.TimeoutIsNotFailed.Contains("NetworkTimeout", StringComparison.Ordinal));
    }

    [Fact]
    public void Unresolved_Created_Or_Initiated_Attempt_Blocks_Another_Attempt()
    {
        var reservation = HotelSupplierReservation.StartPending(HotelBookingId.New(), "test-source", T0);
        reservation.StartAttempt(T0);
        var created = Assert.Throws<InvalidOperationException>(() => reservation.StartAttempt(T0.Plus(Duration.FromSeconds(1))));
        Assert.Contains("unresolved", created.Message, StringComparison.OrdinalIgnoreCase);

        var other = HotelSupplierReservation.StartPending(HotelBookingId.New(), "test-source", T0);
        var initiated = other.StartAttempt(T0);
        other.MarkAttemptInitiated(initiated.Id, T0);
        var blocked = Assert.Throws<InvalidOperationException>(() => other.StartAttempt(T0.Plus(Duration.FromSeconds(1))));
        Assert.Contains("unresolved", blocked.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Failed_Attempt_Allows_A_New_Attempt_Under_Same_Reservation()
    {
        var reservation = HotelSupplierReservation.StartPending(HotelBookingId.New(), "test-source", T0);
        var first = reservation.StartAttempt(T0);
        reservation.FailAttempt(first.Id, T0.Plus(Duration.FromMinutes(1)));
        Assert.Equal(HotelSupplierReservationAttemptStatus.Failed, first.Status);
        Assert.Equal(HotelSupplierReservationStatus.Pending, reservation.Status);

        var second = reservation.StartAttempt(T0.Plus(Duration.FromMinutes(2)));
        Assert.Equal(HotelSupplierReservationAttemptStatus.Created, second.Status);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Confirmed_Reservation_Cannot_Start_Another_Attempt()
    {
        var booking = TwoRoomBooking();
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            null,
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());

        Assert.Throws<InvalidOperationException>(() => reservation.StartAttempt(T0.Plus(Duration.FromMinutes(2))));
    }

    [Fact]
    public void Partial_Room_Confirmation_Cannot_Confirm_Reservation_Or_Booking()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        var partial = Assert.Throws<InvalidOperationException>(() =>
            reservation.ConfirmAttempt(
                attempt.Id,
                T0.Plus(Duration.FromMinutes(1)),
                "src-res-partial",
                null,
                [booking.Rooms[0].Id],
                booking.Rooms.Select(r => r.Id).ToArray()));
        Assert.Contains("Partial", partial.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HotelSupplierReservationStatus.Pending, reservation.Status);
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);

        var issues = booking.CollectConfirmationIssues(
            reservation,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            [booking.Rooms[0].Id],
            snapshot.Monetary.Total,
            true,
            snapshot.Monetary);
        Assert.Contains(HotelBookingReconciliationIssueKind.RoomSetMismatch, issues);
        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativeSupplierReservation(
                reservation,
                T0.Plus(Duration.FromMinutes(1)),
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(),
                snapshot.Monetary.Total,
                true,
                snapshot.Monetary,
                []));
    }

    [Fact]
    public void Monetary_And_Stay_Mismatch_Do_Not_Confirm_And_Do_Not_Rewrite_Snapshot()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            null,
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());

        var moneyIssues = booking.CollectConfirmationIssues(
            reservation,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(),
            Irr(999_999m),
            true,
            snapshot.Monetary);
        Assert.Contains(HotelBookingReconciliationIssueKind.MonetaryMismatch, moneyIssues);

        var currencyIssues = booking.CollectConfirmationIssues(
            reservation,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(),
            new MoneyValue(1_000_000m, CurrencyCode.Parse("USD")),
            true,
            snapshot.Monetary);
        Assert.Contains(HotelBookingReconciliationIssueKind.CurrencyMismatch, currencyIssues);

        var stayIssues = booking.CollectConfirmationIssues(
            reservation,
            booking.Place,
            new LocalDate(2026, 9, 1),
            new LocalDate(2026, 9, 3),
            booking.Rooms.Select(r => r.Id).ToArray(),
            snapshot.Monetary.Total,
            false,
            snapshot.Monetary);
        Assert.Contains(HotelBookingReconciliationIssueKind.StayMismatch, stayIssues);
        Assert.Contains(HotelBookingReconciliationIssueKind.CancellationTermsMismatch, stayIssues);

        var hotelIssues = booking.CollectConfirmationIssues(
            reservation,
            new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000099")),
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(),
            snapshot.Monetary.Total,
            true,
            snapshot.Monetary);
        Assert.Contains(HotelBookingReconciliationIssueKind.HotelMismatch, hotelIssues);

        var blocking = new HotelBookingReconciliationIssue(
            booking.Id,
            HotelBookingReconciliationIssueKind.MonetaryMismatch,
            T0);
        Assert.True(blocking.BlocksConfirmation);
        Assert.False(
            new HotelBookingReconciliationIssue(
                booking.Id,
                HotelBookingReconciliationIssueKind.AmbiguousReservationOutcome,
                T0).BlocksConfirmation);

        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativeSupplierReservation(
                reservation,
                T0.Plus(Duration.FromMinutes(1)),
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(),
                Irr(999_999m),
                true,
                snapshot.Monetary,
                []));
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
    }

    [Fact]
    public void Ownership_Boundary_Keeps_Payment_And_Public_Api_Out()
    {
        Assert.Equal("NONE", HotelReservationOwnershipBoundary.NamedHotelSupplier);
        Assert.Equal("NONE", HotelReservationOwnershipBoundary.ProductionHotelReservationSource);
        Assert.Equal("IHotelReservationSource", HotelReservationOwnershipBoundary.SourcePortName);
        Assert.False(HotelReservationOwnershipBoundary.ProductionFakeReservationSourceImplemented);
        Assert.False(HotelReservationOwnershipBoundary.NamedSupplierSdkImplemented);
        Assert.True(HotelReservationOwnershipBoundary.PaymentRequiredForConfirmation);
        Assert.True(HotelReservationOwnershipBoundary.CancellationExecutionImplemented);
        Assert.False(HotelReservationOwnershipBoundary.PublicReservationApiImplemented);
        Assert.False(HotelReservationOwnershipBoundary.ProcessLocalLockIsAuthority);
        Assert.Equal("Pending, Confirmed, Cancelled", HotelReservationOwnershipBoundary.HotelBookingStatuses);
        Assert.Equal("Created, Initiated, Confirmed, Failed", HotelReservationOwnershipBoundary.AttemptStatuses);
    }
}
