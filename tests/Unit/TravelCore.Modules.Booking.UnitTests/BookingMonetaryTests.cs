using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Money;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingMonetaryTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant QuotedAt = Instant.FromUtc(2026, 8, 18, 11, 0);
    private static readonly Instant ExpiresAt = Instant.FromUtc(2026, 8, 18, 18, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000501"));
    private static readonly Guid QuoteId = Guid.Parse("0198b3e0-0000-7000-8000-000000000502");
    private static readonly Guid SourcePriceId = Guid.Parse("0198b3e0-0000-7000-8000-000000000503");

    [Fact]
    public void Valid_Quote_Creates_Immutable_Snapshot_Without_Changing_BookingStatus()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.AcceptQuote(CreateFacts(), Now);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotNull(booking.MonetarySnapshot);
        Assert.Equal(QuoteId, booking.MonetarySnapshot.QuoteReference.LogicalId);
        Assert.Equal(110m, booking.MonetarySnapshot.Total.Amount);
        Assert.Equal("IRR", booking.MonetarySnapshot.Total.Currency.Value);
        Assert.Equal(2, booking.MonetarySnapshot.Components.Count);
        Assert.Equal(new[] { "Pending", "Confirmed", "Cancelled" }, Enum.GetNames<BookingStatus>());
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
        Assert.Equal("Price != Quote", BookingMonetaryBoundary.PriceIsNotQuote);
        Assert.Equal("Quote != BookingMonetarySnapshot", BookingMonetaryBoundary.QuoteIsNotBookingMonetarySnapshot);
    }

    [Fact]
    public void Expired_Quote_Is_Rejected()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        var expired = CreateFacts(expiresAt: Now);
        Assert.Throws<InvalidOperationException>(() => booking.AcceptQuote(expired, Now));
        Assert.Null(booking.MonetarySnapshot);
    }

    [Fact]
    public void Mismatched_TourDeparture_Quote_Is_Rejected()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        var other = CreateFacts(targetId: Guid.Parse("0198b3e0-0000-7000-8000-000000000599"));
        Assert.Throws<InvalidOperationException>(() => booking.AcceptQuote(other, Now));
    }

    [Fact]
    public void Same_Quote_Acceptance_Is_Idempotent()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.AcceptQuote(CreateFacts(), Now);
        var first = booking.MonetarySnapshot;
        booking.AcceptQuote(CreateFacts(), Now.Plus(Duration.FromMinutes(1)));
        Assert.Same(first, booking.MonetarySnapshot);
        Assert.Equal(Now, booking.MonetarySnapshot!.AcceptedAt);
    }

    [Fact]
    public void Different_Quote_Cannot_Overwrite_Existing_Snapshot()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.AcceptQuote(CreateFacts(), Now);
        var otherQuote = CreateFacts(quoteId: Guid.Parse("0198b3e0-0000-7000-8000-000000000577"));
        Assert.Throws<InvalidOperationException>(() => booking.AcceptQuote(otherQuote, Now));
        Assert.Equal(QuoteId, booking.MonetarySnapshot!.QuoteReference.LogicalId);
    }

    [Fact]
    public void Later_Quote_Fact_Mutation_Does_Not_Rewrite_Accepted_Snapshot()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        var facts = CreateFacts(baseAmount: 100m);
        booking.AcceptQuote(facts, Now);
        var mutated = CreateFacts(baseAmount: 999m);
        Assert.Equal(110m, booking.MonetarySnapshot!.Total.Amount);
        Assert.Equal(100m, booking.MonetarySnapshot.Components.Single(c => c.Kind == BookingMonetaryComponentKind.Base).Money.Amount);
        Assert.Equal(999m, mutated.Components[0].Money.Amount);
    }

    [Fact]
    public void Service_Rejects_Missing_Quote_And_Does_Not_Accept_Client_Amounts()
    {
        var accept = typeof(BookingQuoteService).GetMethod(nameof(BookingQuoteService.AcceptQuoteAsync));
        Assert.NotNull(accept);
        var names = accept!.GetParameters().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "bookingId", "quoteId", "now", "cancellationToken" }, names);
        Assert.DoesNotContain("totalAmount", names);
        Assert.DoesNotContain("currencyCode", names);
        Assert.Equal("BookingMonetarySnapshot != PaymentAmount", BookingMonetaryBoundary.BookingMonetarySnapshotIsNotPaymentAmount);
        Assert.Equal("BudgetPreference != BookingMonetarySnapshot", BookingMonetaryBoundary.BudgetPreferenceIsNotBookingMonetarySnapshot);
        Assert.Equal("QuoteExpiresAt != CapacityHold.ExpiresAt", BookingMonetaryBoundary.QuoteExpiresAtIsNotCapacityHoldExpiresAt);
        Assert.False(BookingMonetaryBoundary.FxImplemented);
    }

    private static AuthoritativeQuoteFacts CreateFacts(
        Guid? quoteId = null,
        Guid? targetId = null,
        Instant? expiresAt = null,
        decimal baseAmount = 100m)
    {
        return AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(quoteId ?? QuoteId),
            SourcePriceId,
            BookingOwnershipBoundary.InitialTarget,
            targetId ?? Departure.LogicalId,
            QuotedAt,
            expiresAt ?? ExpiresAt,
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new MoneyValue(baseAmount, "IRR"),
                    0,
                    "BASE",
                    "Base fare"),
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Tax,
                    new MoneyValue(10m, "IRR"),
                    1,
                    "TAX",
                    "Tax")
            ]);
    }
}
