using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure.Services;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingCancellationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 15, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000601"));

    [Fact]
    public void Pending_Cancellation_Preserves_People_And_Monetary_Snapshots()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.SetContact(BookingContactSnapshot.Create("Booker", "b@example.com"));
        booking.AddPassenger("A", "One", TravelerCategory.Adult, null);
        booking.AcceptQuote(QuoteFacts(), Now);
        booking.CancelPending(Now.Plus(Duration.FromMinutes(1)));

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal("Booker", booking.Contact!.DisplayName);
        Assert.Equal(1, booking.PassengerCount);
        Assert.NotNull(booking.MonetarySnapshot);
        Assert.Equal(50m, booking.MonetarySnapshot.Total.Amount);
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
        Assert.Equal("Booking != Payment", BookingOrchestrationBoundary.BookingIsNotPayment);
        Assert.Equal("BookingCancelled != PaymentRefunded", BookingOrchestrationBoundary.BookingCancelledIsNotPaymentRefunded);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", BookingOrchestrationBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.Equal("DEFERRED to Payment integration", BookingOrchestrationBoundary.ExecutableConfirmWorkflow);
        Assert.False(BookingOrchestrationBoundary.PaymentDrivenConfirmationImplemented);
        Assert.False(BookingOrchestrationBoundary.CallerControlledPaymentBooleanImplemented);
        Assert.False(BookingOrchestrationBoundary.FakePaymentImplemented);
    }

    [Fact]
    public void Cancellation_Service_Has_No_Payment_Success_Boolean()
    {
        var cancel = typeof(BookingCancellationService).GetMethod(nameof(BookingCancellationService.CancelPendingAsync));
        Assert.NotNull(cancel);
        var names = cancel!.GetParameters().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "bookingId", "now", "cancellationToken" }, names);
        Assert.DoesNotContain("paymentSucceeded", names);
        Assert.DoesNotContain("isPaid", names);
        Assert.DoesNotContain("Confirm", typeof(BookingAggregate).GetMethods().Select(m => m.Name));
        Assert.False(BookingOwnershipBoundary.PaymentIntegrationImplemented);
    }

    private static AuthoritativeQuoteFacts QuoteFacts()
    {
        return AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(Guid.Parse("0198b3e0-0000-7000-8000-000000000602")),
            Guid.Parse("0198b3e0-0000-7000-8000-000000000603"),
            BookingOwnershipBoundary.InitialTarget,
            Departure.LogicalId,
            Now.Minus(Duration.FromMinutes(10)),
            Now.Plus(Duration.FromHours(4)),
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new TravelCore.Money.Money(50m, "EUR"),
                    0,
                    "BASE",
                    "Base")
            ]);
    }
}
