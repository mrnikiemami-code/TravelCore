using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingPaymentConfirmationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 16, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000610"));

    [Fact]
    public void Authoritative_Payment_Success_Confirms_Pending_Booking()
    {
        var booking = EligiblePending();
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1)));

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
        Assert.Null(typeof(BookingAggregate).GetMethod("SetConfirmed"));
        Assert.False(BookingLifecycleBoundary.UnrestrictedConfirmationImplemented);
        Assert.True(BookingOrchestrationBoundary.PaymentDrivenConfirmationImplemented);
        Assert.True(BookingOrchestrationBoundary.ConfirmationRecoveryEvidenceImplemented);
        Assert.Equal("RecoveryIssue != Refund", BookingOrchestrationBoundary.RecoveryIssueIsNotRefund);
        Assert.Equal(
            new[]
            {
                BookingConfirmationRecoveryReason.ExpiredHold,
                BookingConfirmationRecoveryReason.ReleasedHold,
                BookingConfirmationRecoveryReason.CancelledBooking,
                BookingConfirmationRecoveryReason.MonetaryMismatch,
                BookingConfirmationRecoveryReason.MissingMonetarySnapshot,
                BookingConfirmationRecoveryReason.MissingPeoplePrerequisites,
            },
            Enum.GetValues<BookingConfirmationRecoveryReason>());
        Assert.Null(typeof(BookingDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Booking.Domain.Refund"));
    }

    [Fact]
    public void Repeated_Confirmation_Is_Idempotent()
    {
        var booking = EligiblePending();
        var first = Now.Plus(Duration.FromMinutes(1));
        booking.ConfirmFromAuthoritativePaymentSuccess(first);
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(2)));
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(first, booking.StatusChangedAt);
    }

    [Fact]
    public void Cancelled_Booking_Cannot_Confirm_Or_Reopen()
    {
        var booking = EligiblePending();
        booking.CancelPending(Now.Plus(Duration.FromMinutes(1)));
        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(2))));
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Missing_Monetary_Snapshot_Cannot_Confirm()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.SetContact(BookingContactSnapshot.Create("A", "a@example.com"));
        booking.AddPassenger("A", "One", TravelerCategory.Adult, null);
        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1))));
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Recovery_Issue_Is_Not_Refund_And_Is_Booking_Owned()
    {
        var issue = BookingConfirmationRecoveryIssue.Create(
            BookingId.New(),
            Guid.CreateVersion7(),
            BookingConfirmationRecoveryReason.ExpiredHold,
            Now);
        Assert.Equal(BookingConfirmationRecoveryReason.ExpiredHold, issue.Reason);
        Assert.NotEqual(Guid.Empty, issue.PaymentId);
        Assert.Equal("RecoveryIssue != Refund", BookingOrchestrationBoundary.RecoveryIssueIsNotRefund);
        Assert.Equal("RecoveryIssue != PaymentStatus", BookingOrchestrationBoundary.RecoveryIssueIsNotPaymentStatus);
        Assert.Equal("RecoveryIssue != BookingStatus", BookingOrchestrationBoundary.RecoveryIssueIsNotBookingStatus);
        Assert.False(BookingLifecycleBoundary.RefundedStatusImplemented);
    }

    private static BookingAggregate EligiblePending()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.SetContact(BookingContactSnapshot.Create("A", "a@example.com"));
        booking.AddPassenger("A", "One", TravelerCategory.Adult, 1);
        booking.AcceptQuote(CreateFacts(), Now);
        return booking;
    }

    private static AuthoritativeQuoteFacts CreateFacts()
    {
        return AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(Guid.Parse("0198b3e0-0000-7000-8000-000000000611")),
            Guid.Parse("0198b3e0-0000-7000-8000-000000000612"),
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
}
