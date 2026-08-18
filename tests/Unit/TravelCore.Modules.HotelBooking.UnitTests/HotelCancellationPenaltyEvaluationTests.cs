using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.UnitTests;

public sealed class HotelCancellationPenaltyEvaluationTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly HotelPlaceReference Place =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000021"));

    [Fact]
    public void Penalty_Zero_Is_FullRefund()
    {
        var booking = Booking();
        var snapshot = Accept(booking, rules:
        [
            new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(0m)),
            new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, Irr(1_000_000m)),
        ]);
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            T0);
        Assert.Equal(HotelCancellationPenaltyEvaluationKind.FullRefund, evaluation.Kind);
        Assert.Equal(0m, evaluation.Penalty!.Amount);
        Assert.Equal(1_000_000m, evaluation.RefundAmount!.Amount);
        Assert.Equal("IRR", evaluation.Penalty.Currency.Value);
        Assert.True(evaluation.IsExecutable);
    }

    [Fact]
    public void Penalty_Equals_Total_Is_NoRefund()
    {
        var booking = Booking();
        var snapshot = Accept(booking, rules:
        [
            new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(0m)),
            new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, Irr(1_000_000m)),
        ]);
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            T0.Plus(Duration.FromDays(2)));
        Assert.Equal(HotelCancellationPenaltyEvaluationKind.NoRefund, evaluation.Kind);
        Assert.Equal(1_000_000m, evaluation.Penalty!.Amount);
        Assert.Equal(0m, evaluation.RefundAmount!.Amount);
        Assert.True(evaluation.IsExecutable);
    }

    [Fact]
    public void Partial_Penalty_Is_Unsupported_And_Does_Not_Round_To_Boundary()
    {
        var booking = Booking();
        var snapshot = Accept(booking, rules:
        [
            new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), Irr(200_000m)),
        ]);
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            T0);
        Assert.Equal(HotelCancellationPenaltyEvaluationKind.PartialRefundRequiredUnsupported, evaluation.Kind);
        Assert.Equal(200_000m, evaluation.Penalty!.Amount);
        Assert.Equal(800_000m, evaluation.RefundAmount!.Amount);
        Assert.False(evaluation.IsExecutable);
        Assert.NotEqual(0m, evaluation.Penalty.Amount);
        Assert.NotEqual(snapshot.Monetary.Total.Amount, evaluation.Penalty.Amount);
    }

    [Fact]
    public void RequestedAt_Selects_Rule_Not_Processing_Completion_Time()
    {
        var booking = Booking();
        var snapshot = Accept(booking, rules:
        [
            new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromHours(1)), Irr(0m)),
            new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromHours(1)), null, Irr(1_000_000m)),
        ]);
        var requestedAt = T0.Plus(Duration.FromMinutes(30));
        var afterCutoff = T0.Plus(Duration.FromHours(2));
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            requestedAt);
        Assert.Equal(HotelCancellationPenaltyEvaluationKind.FullRefund, evaluation.Kind);
        var later = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            afterCutoff);
        Assert.Equal(HotelCancellationPenaltyEvaluationKind.NoRefund, later.Kind);
    }

    [Fact]
    public void No_Matching_Rule_Is_Not_Executable()
    {
        var booking = Booking();
        var snapshot = Accept(booking, rules:
        [
            new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), T0.Plus(Duration.FromDays(2)), Irr(0m)),
        ]);
        var evaluation = HotelCancellationPenaltyEvaluator.Evaluate(
            snapshot.CancellationPolicy,
            snapshot.Monetary,
            T0);
        Assert.Equal(HotelCancellationPenaltyEvaluationKind.NoDeterministicRule, evaluation.Kind);
        Assert.False(evaluation.IsExecutable);
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

    private static HotelRateOfferSnapshot Accept(
        Stay booking,
        IReadOnlyList<HotelCancellationPenaltyRuleDraft> rules)
    {
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        return HotelRateOfferSnapshot.Accept(
            booking,
            T0,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            "offer-eval",
            T0.Minus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            Irr(1_000_000m),
            [
                new HotelRoomRateLine(rooms[0].Id, Irr(400_000m), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, Irr(600_000m), "sel-2", "rate-2", "BB"),
            ],
            rules,
            propertyTimeZoneId: "Asia/Tehran");
    }
}
