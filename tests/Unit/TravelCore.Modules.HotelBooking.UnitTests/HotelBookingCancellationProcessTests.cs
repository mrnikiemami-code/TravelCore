using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelBookingCancellationProcessTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly HotelPlaceReference Place =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));

    [Fact]
    public void Attempt_Lifecycle_Created_Initiated_Confirmed_Failed_Timeout_Stays_Initiated()
    {
        var cancellation = Start(fullRefund: true);
        var attempt = cancellation.StartAttempt(T0);
        Assert.Equal(HotelSupplierCancellationAttemptStatus.Created, attempt.Status);
        Assert.Equal(HotelBookingCancellationStatus.SupplierCancellationPending, cancellation.Status);

        cancellation.MarkAttemptInitiated(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
        Assert.Equal(HotelSupplierCancellationAttemptStatus.Initiated, attempt.Status);
        Assert.True(attempt.IsUnresolved);
        Assert.Throws<InvalidOperationException>(() => cancellation.StartAttempt(T0.Plus(Duration.FromSeconds(2))));

        cancellation.FailAttempt(attempt.Id, T0.Plus(Duration.FromSeconds(3)));
        Assert.Equal(HotelSupplierCancellationAttemptStatus.Failed, attempt.Status);
        Assert.Equal(HotelBookingCancellationStatus.SupplierCancellationPending, cancellation.Status);

        var retry = cancellation.StartAttempt(T0.Plus(Duration.FromSeconds(4)));
        cancellation.MarkAttemptInitiated(retry.Id, T0.Plus(Duration.FromSeconds(5)));
        Assert.Equal(HotelSupplierCancellationAttemptStatus.Initiated, retry.Status);
        cancellation.ConfirmAttempt(retry.Id, T0.Plus(Duration.FromSeconds(6)));
        Assert.Equal(HotelSupplierCancellationAttemptStatus.Confirmed, retry.Status);
        Assert.Equal(HotelBookingCancellationStatus.RefundPending, cancellation.Status);
        Assert.Throws<InvalidOperationException>(() => cancellation.StartAttempt(T0.Plus(Duration.FromSeconds(7))));
    }

    [Fact]
    public void NoRefund_Completes_Without_RefundPending()
    {
        var cancellation = Start(fullRefund: false);
        var attempt = cancellation.StartAttempt(T0);
        cancellation.ConfirmAttempt(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
        Assert.Equal(HotelBookingCancellationStatus.Completed, cancellation.Status);
        Assert.NotNull(cancellation.CompletedAt);
    }

    [Fact]
    public void Refund_Success_Completes_FullRefund_Once()
    {
        var cancellation = Start(fullRefund: true);
        var attempt = cancellation.StartAttempt(T0);
        cancellation.ConfirmAttempt(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
        cancellation.CompleteFromAuthoritativeRefundSuccess(T0.Plus(Duration.FromMinutes(1)));
        Assert.Equal(HotelBookingCancellationStatus.Completed, cancellation.Status);
        var completedAt = cancellation.CompletedAt;
        cancellation.CompleteFromAuthoritativeRefundSuccess(T0.Plus(Duration.FromMinutes(2)));
        Assert.Equal(completedAt, cancellation.CompletedAt);
    }

    [Fact]
    public void Constrained_Cancel_Requires_Confirmed_Then_Authoritative_Reservation_Cancelled()
    {
        var booking = Booking();
        var snapshot = Accept(booking);
        var reservation = ConfirmedReservation(booking);
        var evidence = HotelBookingPaymentEvidence.Record(
            booking.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0);
        booking.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation, evidence, T0.Plus(Duration.FromMinutes(1)),
            booking.Place, booking.CheckInDate, booking.CheckOutDate,
            booking.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        Assert.Equal(HotelBookingStatus.Confirmed, booking.Status);
        Assert.Throws<InvalidOperationException>(() =>
            booking.CancelFromAuthoritativeSupplierCancellation(reservation, T0.Plus(Duration.FromMinutes(2))));

        reservation.CancelFromAuthoritativeSupplierCancellation(T0.Plus(Duration.FromMinutes(2)));
        booking.CancelFromAuthoritativeSupplierCancellation(reservation, T0.Plus(Duration.FromMinutes(2)));
        Assert.Equal(HotelBookingStatus.Cancelled, booking.Status);
        Assert.Equal(HotelSupplierReservationStatus.Cancelled, reservation.Status);
        booking.CancelFromAuthoritativeSupplierCancellation(reservation, T0.Plus(Duration.FromMinutes(3)));
        Assert.Equal(T0.Plus(Duration.FromMinutes(2)), booking.CancelledAt);
    }

    [Fact]
    public void R6_Compensation_Cannot_Cancel_Confirmed_And_R7_Cannot_Cancel_Pending()
    {
        var pending = Booking();
        Assert.Throws<InvalidOperationException>(() =>
        {
            var reservation = ConfirmedReservation(pending);
            reservation.CancelFromAuthoritativeSupplierCancellation(T0);
            pending.CancelFromAuthoritativeSupplierCancellation(reservation, T0);
        });

        var confirmed = Booking();
        var snapshot = Accept(confirmed);
        var reservation = ConfirmedReservation(confirmed);
        confirmed.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            reservation,
            HotelBookingPaymentEvidence.Record(confirmed.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0),
            T0.Plus(Duration.FromMinutes(1)),
            confirmed.Place, confirmed.CheckInDate, confirmed.CheckOutDate,
            confirmed.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        Assert.Throws<InvalidOperationException>(() =>
            confirmed.CancelFromAuthoritativePaymentCompensation(T0.Plus(Duration.FromMinutes(2))));
        Assert.Equal(HotelBookingStatus.Confirmed, confirmed.Status);
    }

    [Fact]
    public void Cross_Booking_Reservation_Cannot_Cancel()
    {
        var a = Booking();
        var b = Booking();
        var snapshot = Accept(a);
        var reservationB = ConfirmedReservation(b);
        var evidence = HotelBookingPaymentEvidence.Record(a.Id, Guid.CreateVersion7(), 1_000_000m, "IRR", T0);
        a.ConfirmFromAuthoritativePaymentAndSupplierEvidence(
            ConfirmedReservation(a), evidence, T0.Plus(Duration.FromMinutes(1)),
            a.Place, a.CheckInDate, a.CheckOutDate,
            a.Rooms.Select(r => r.Id).ToArray(), snapshot.Monetary.Total, true,
            snapshot.Monetary, []);
        reservationB.CancelFromAuthoritativeSupplierCancellation(T0.Plus(Duration.FromMinutes(2)));
        Assert.Throws<InvalidOperationException>(() =>
            a.CancelFromAuthoritativeSupplierCancellation(reservationB, T0.Plus(Duration.FromMinutes(2))));
        Assert.Equal(HotelBookingStatus.Confirmed, a.Status);
    }

    [Fact]
    public void Status_Enums_Remain_Minimal()
    {
        Assert.Equal(
            new[] { "Requested", "SupplierCancellationPending", "RefundPending", "Completed" },
            Enum.GetNames<HotelBookingCancellationStatus>());
        Assert.Equal(
            new[] { "Created", "Initiated", "Confirmed", "Failed" },
            Enum.GetNames<HotelSupplierCancellationAttemptStatus>());
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("CancellationPending", Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("Cancelling", Enum.GetNames<HotelBookingStatus>());
        Assert.DoesNotContain("RefundPending", Enum.GetNames<HotelBookingStatus>());
        Assert.Null(typeof(Stay).GetMethod("Cancel"));
        Assert.Null(typeof(Stay).GetMethod("SetCancelled"));
        Assert.Null(typeof(Stay).GetMethod("ForceCancel"));
    }

    private static HotelBookingCancellation Start(bool fullRefund)
    {
        var evaluation = fullRefund
            ? HotelCancellationPenaltyEvaluation.FullRefund(Irr(1_000_000m))
            : HotelCancellationPenaltyEvaluation.NoRefund(Irr(1_000_000m));
        return HotelBookingCancellation.StartRequested(
            HotelBookingId.New(),
            Guid.CreateVersion7(),
            T0,
            evaluation);
    }

    private static Stay Booking() =>
        Stay.Create(
            Place,
            new LocalDate(2026, 8, 20),
            new LocalDate(2026, 8, 22),
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

    private static HotelRateOfferSnapshot Accept(Stay booking)
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
            Irr(1_000_000m),
            [
                new HotelRoomRateLine(rooms[0].Id, Irr(400_000m), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, Irr(600_000m), "sel-2", "rate-2", "BB"),
            ],
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(0m)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, Irr(1_000_000m)),
            ]);
    }

    private static HotelSupplierReservation ConfirmedReservation(Stay booking)
    {
        var reservation = HotelSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            $"src-res-{booking.Id.Value:N}",
            "CNF-1",
            booking.Rooms.Select(r => r.Id).ToArray(),
            booking.Rooms.Select(r => r.Id).ToArray());
        return reservation;
    }
}
