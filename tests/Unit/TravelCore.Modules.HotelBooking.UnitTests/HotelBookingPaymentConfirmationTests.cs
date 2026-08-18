using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingPaymentConfirmationTests
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

    private static HotelSupplierReservation ConfirmedReservation(Stay booking)
    {
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            "CONF-1",
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());
        return reservation;
    }

    private static HotelBookingPaymentEvidence MatchingEvidence(Stay booking, HotelRateOfferSnapshot snapshot) =>
        HotelBookingPaymentEvidence.Record(
            booking.Id,
            Guid.CreateVersion7(),
            snapshot.Monetary.Total.Amount,
            snapshot.Monetary.CurrencyCode.Value,
            T0.Plus(Duration.FromMinutes(1)));

    [Fact]
    public void Valid_Payment_Evidence_Is_Recorded_And_Matches_Snapshot()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var evidence = MatchingEvidence(booking, snapshot);
        Assert.Equal(booking.Id, evidence.HotelBookingId);
        Assert.True(evidence.MatchesMonetarySnapshot(snapshot.Monetary));
        Assert.Equal("IRR", evidence.CurrencyCode);
    }

    [Fact]
    public void Amount_And_Currency_Mismatch_Does_Not_Confirm()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var reservation = ConfirmedReservation(booking);
        var amountMismatch = HotelBookingPaymentEvidence.Record(
            booking.Id, Guid.CreateVersion7(), 999m, "IRR", T0);
        var currencyMismatch = HotelBookingPaymentEvidence.Record(
            booking.Id, Guid.CreateVersion7(), 1_000_000m, "USD", T0);

        Assert.False(amountMismatch.MatchesMonetarySnapshot(snapshot.Monetary));
        Assert.False(currencyMismatch.MatchesMonetarySnapshot(snapshot.Monetary));
        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
                reservation, amountMismatch, T0.Plus(Duration.FromMinutes(1)),
                booking.Place, booking.CheckInDate, booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
                snapshot.Monetary, []));
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);

        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
                reservation, currencyMismatch, T0.Plus(Duration.FromMinutes(1)),
                booking.Place, booking.CheckInDate, booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
                snapshot.Monetary, []));
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Payment_Only_Leaves_Pending()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var evidence = MatchingEvidence(booking, snapshot);
        Assert.True(evidence.MatchesMonetarySnapshot(snapshot.Monetary));
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Null(booking.ConfirmedAt);
    }

    [Fact]
    public void Supplier_Only_Leaves_Pending()
    {
        var booking = TwoRoomBooking();
        var reservation = ConfirmedReservation(booking);
        Assert.Equal(HotelSupplierReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(HotelBookingStatus.Pending, booking.Status);
        Assert.Null(typeof(Stay).GetMethod("Confirm"));
        Assert.Null(typeof(Stay).GetMethod("SetConfirmed"));
        Assert.Null(typeof(Stay).GetMethod("ForceConfirm"));
        Assert.Null(typeof(Stay).GetMethod("Cancel"));
        Assert.Null(typeof(Stay).GetMethod("SetCancelled"));
        Assert.Null(typeof(Stay).GetMethod("ForceCancel"));
    }

    [Fact]
    public void Both_Evidences_Confirm_And_Duplicate_Confirms_Once()
    {
        var booking = TwoRoomBooking();
        var snapshot = AcceptOffer(booking);
        var reservation = ConfirmedReservation(booking);
        var evidence = MatchingEvidence(booking, snapshot);
        booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation, evidence, T0.Plus(Duration.FromMinutes(1)),
            booking.Place, booking.CheckInDate, booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        Assert.Equal(HotelBookingStatus.Confirmed, booking.Status);
        var firstConfirmedAt = booking.ConfirmedAt;
        booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation, evidence, T0.Plus(Duration.FromMinutes(5)),
            booking.Place, booking.CheckInDate, booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        Assert.Equal(firstConfirmedAt, booking.ConfirmedAt);
    }

    [Fact]
    public void Cancelled_Never_Reopens()
    {
        var booking = TwoRoomBooking();
        booking.CancelFromAuthoritativePaymentCompensation(T0);
        Assert.Equal(HotelBookingStatus.Cancelled, booking.Status);
        booking.CancelFromAuthoritativePaymentCompensation(T0.Plus(Duration.FromMinutes(1)));
        Assert.Equal(T0, booking.CancelledAt);

        var snapshot = AcceptOffer(booking);
        var reservation = ConfirmedReservation(booking);
        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
                reservation, MatchingEvidence(booking, snapshot), T0.Plus(Duration.FromMinutes(2)),
                booking.Place, booking.CheckInDate, booking.CheckOutDate,
                booking.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
                snapshot.Monetary, []));
        Assert.Equal(HotelBookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Compensation_Refund_Success_Cancels_Pending_Only()
    {
        var pending = TwoRoomBooking();
        pending.CancelFromAuthoritativePaymentCompensation(T0);
        Assert.Equal(HotelBookingStatus.Cancelled, pending.Status);

        var confirmed = TwoRoomBooking();
        var snapshot = AcceptOffer(confirmed);
        var reservation = ConfirmedReservation(confirmed);
        confirmed.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation, MatchingEvidence(confirmed, snapshot), T0.Plus(Duration.FromMinutes(1)),
            confirmed.Place, confirmed.CheckInDate, confirmed.CheckOutDate,
            confirmed.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        Assert.Equal(HotelBookingStatus.Confirmed, confirmed.Status);
        Assert.Throws<InvalidOperationException>(() =>
            confirmed.CancelFromAuthoritativePaymentCompensation(T0.Plus(Duration.FromMinutes(2))));
        Assert.Equal(HotelBookingStatus.Confirmed, confirmed.Status);
    }

    [Fact]
    public void Compensation_Reasons_Are_The_Minimal_Authoritative_Set()
    {
        Assert.Equal(
            new[]
            {
                "HoldExpired",
                "HoldReleased",
                "SupplierReservationNotCreated",
                "SupplierReservationCancelled",
                "MonetaryMismatch",
                "CurrencyMismatch",
                "RoomSetMismatch",
                "StayMismatch",
                "HotelMismatch",
                "CancellationTermsMismatch",
            },
            Enum.GetNames<HotelBookingPaymentCompensationReason>());
        Assert.DoesNotContain("HandlerCrash", Enum.GetNames<HotelBookingPaymentCompensationReason>());
        Assert.DoesNotContain("Timeout", Enum.GetNames<HotelBookingPaymentCompensationReason>());
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("AwaitingPayment", Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("Paid", Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("Refunding", Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("Compensating", Enum.GetNames<HotelBookingStatus>());
    }
}
