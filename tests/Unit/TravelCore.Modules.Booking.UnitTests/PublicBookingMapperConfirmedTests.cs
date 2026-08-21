using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

/// <summary>
/// TC-P34-T005 — public Confirmed flag must reflect BookingStatus (not hardcoded false).
/// Payment compose uses booking.Confirmed; Payment must not invent Confirm.
/// </summary>
public sealed class PublicBookingMapperConfirmedTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 21, 18, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000710"));

    [Fact]
    public void ToRead_Pending_Maps_Confirmed_False()
    {
        var booking = EligiblePending();
        var read = PublicBookingMapper.ToRead(booking, hold: null);

        Assert.Equal(BookingStatus.Pending.ToString(), read.Status);
        Assert.False(read.Confirmed);
    }

    [Fact]
    public void ToRead_Confirmed_Maps_Confirmed_True()
    {
        var booking = EligiblePending();
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1)));

        var read = PublicBookingMapper.ToRead(booking, hold: null);

        Assert.Equal(BookingStatus.Confirmed.ToString(), read.Status);
        Assert.True(read.Confirmed);
    }

    [Fact]
    public void ToInitiation_Pending_Maps_Confirmed_False()
    {
        var booking = EligiblePending();
        var response = PublicBookingMapper.ToInitiation(booking, hold: null, rawAccessToken: "token");

        Assert.Equal(BookingStatus.Pending.ToString(), response.Status);
        Assert.False(response.Confirmed);
    }

    [Fact]
    public void ToInitiation_Confirmed_Maps_Confirmed_True()
    {
        var booking = EligiblePending();
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1)));

        var response = PublicBookingMapper.ToInitiation(booking, hold: null, rawAccessToken: null);

        Assert.Equal(BookingStatus.Confirmed.ToString(), response.Status);
        Assert.True(response.Confirmed);
    }

    [Fact]
    public void Payment_Compose_Reflects_Booking_Confirmed_Without_Inventing_Confirm()
    {
        var booking = EligiblePending();
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1)));
        var read = PublicBookingMapper.ToRead(booking, hold: null);

        // Mirrors PublicBookingEndpoints.Compose — Booking-owned flag only.
        var composed = new PublicBookingPaymentRead(
            read.BookingId,
            read.Status,
            read.Confirmed,
            PaymentId: Guid.CreateVersion7(),
            PaymentStatus: "Succeeded",
            Amount: read.Monetary?.TotalAmount,
            CurrencyCode: read.Monetary?.Currency,
            ProviderInitiationPossible: false,
            LatestAttemptStatus: "Succeeded",
            RefundStatus: null,
            SafeAction: "Succeeded",
            RedirectUri: null);

        Assert.True(composed.BookingConfirmed);
        Assert.Equal(BookingStatus.Confirmed.ToString(), composed.BookingStatus);
    }

    private static BookingAggregate EligiblePending()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.SetContact(BookingContactSnapshot.Create("A", "a@example.com"));
        booking.AddPassenger("A", "One", TravelerCategory.Adult, 1);
        booking.AcceptQuote(CreateFacts(), Now);
        return booking;
    }

    private static AuthoritativeQuoteFacts CreateFacts() =>
        AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(Guid.Parse("0198b3e0-0000-7000-8000-000000000711")),
            Guid.Parse("0198b3e0-0000-7000-8000-000000000712"),
            BookingOwnershipBoundary.InitialTarget,
            Departure.LogicalId,
            Now.Minus(Duration.FromMinutes(10)),
            Now.Plus(Duration.FromHours(6)),
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new TravelCore.Money.Money(100m, "IRR"),
                    0,
                    "BASE",
                    "Base")
            ]);
}
