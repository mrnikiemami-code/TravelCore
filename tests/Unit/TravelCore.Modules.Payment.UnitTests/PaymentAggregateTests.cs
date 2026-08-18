using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentAggregateTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 6, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000220"));

    [Fact]
    public void Create_Starts_Pending_With_BookingReference_And_Uuidv7_Id()
    {
        var payment = PaymentAggregate.Create(Booking, Now);

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(Booking, payment.Booking);
        Assert.Equal(Now, payment.CreatedAt);
        Assert.Equal(Now, payment.StatusChangedAt);
        Assert.Null(payment.SucceededAt);
        Assert.Empty(payment.Attempts);
        Assert.Equal(0, payment.Version);
        Assert.NotEqual(Guid.Empty, payment.Id.Value);
        Assert.Equal(7, payment.Id.Value.Version);
        Assert.Equal(
            new[] { PaymentStatus.Pending, PaymentStatus.Succeeded },
            Enum.GetValues<PaymentStatus>());
        Assert.Equal(
            new[]
            {
                PaymentAttemptStatus.Created,
                PaymentAttemptStatus.Initiated,
                PaymentAttemptStatus.Succeeded,
                PaymentAttemptStatus.Failed,
            },
            Enum.GetValues<PaymentAttemptStatus>());
        var methodNames = typeof(PaymentAggregate).GetMethods(
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToArray();
        Assert.DoesNotContain("SetStatus", methodNames);
        Assert.DoesNotContain("SetAttemptStatus", methodNames);
        Assert.DoesNotContain("MarkSucceeded", methodNames);
        Assert.DoesNotContain("SetSucceeded", methodNames);
        Assert.DoesNotContain("CompletePayment", methodNames);
        Assert.DoesNotContain("MarkSucceededFromClient", methodNames);
        Assert.False(PaymentLifecycleBoundary.CallerControlledSuccessImplemented);
        Assert.False(PaymentLifecycleBoundary.PaymentFailedStatusImplemented);
        Assert.False(PaymentLifecycleBoundary.PaymentRefundedStatusImplemented);
    }

    [Fact]
    public void Create_Requires_NonEmpty_BookingReference()
    {
        Assert.Throws<ArgumentException>(() => PaymentAggregate.Create(new BookingReference(Guid.Empty), Now));
    }

    [Fact]
    public void CreateAttempt_Starts_Created_And_Keeps_Payment_Pending()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);

        Assert.Equal(PaymentAttemptStatus.Created, attempt.Status);
        Assert.Equal(Now, attempt.CreatedAt);
        Assert.Null(attempt.InitiatedAt);
        Assert.Equal(7, attempt.Id.Value.Version);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(1, payment.Version);
        Assert.Single(payment.Attempts);
        Assert.Equal("Created != Provider payment created successfully", PaymentLifecycleBoundary.CreatedIsNotProviderCreated);
    }

    [Fact]
    public void Failed_Attempt_Does_Not_Fail_Logical_Payment_And_Allows_Retry()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var first = payment.CreateAttempt(Now);
        payment.InitiateAttempt(first.Id, Instant.FromUtc(2026, 8, 18, 6, 5));
        payment.RecordAttemptFailure(first.Id, Instant.FromUtc(2026, 8, 18, 6, 10));

        Assert.Equal(PaymentAttemptStatus.Failed, first.Status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal("Failed PaymentAttempt != Failed Payment", PaymentLifecycleBoundary.FailedAttemptIsNotFailedPayment);

        var second = payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 6, 15));
        Assert.Equal(payment.Id, payment.Id);
        Assert.Equal(2, payment.Attempts.Count);
        Assert.Equal(PaymentAttemptStatus.Created, second.Status);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Terminal_Attempt_Cannot_Reopen()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordAttemptFailure(attempt.Id, Instant.FromUtc(2026, 8, 18, 6, 20));

        Assert.Throws<InvalidOperationException>(() =>
            payment.InitiateAttempt(attempt.Id, Instant.FromUtc(2026, 8, 18, 6, 21)));
        payment.RecordAttemptFailure(attempt.Id, Instant.FromUtc(2026, 8, 18, 6, 22));
        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);
    }

    [Fact]
    public void Authoritative_Success_Is_Irreversible_And_Blocks_Further_Attempts()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        var succeededAt = Instant.FromUtc(2026, 8, 18, 6, 30);
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, succeededAt);

        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(PaymentAttemptStatus.Succeeded, attempt.Status);
        Assert.Equal(succeededAt, payment.SucceededAt);
        Assert.Equal(succeededAt, payment.StatusChangedAt);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", PaymentLifecycleBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.Throws<InvalidOperationException>(() => payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 6, 35)));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Instant.FromUtc(2026, 8, 18, 6, 36));
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(succeededAt, payment.SucceededAt);
    }

    [Fact]
    public void Cannot_Produce_Two_Successful_Attempts()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var first = payment.CreateAttempt(Now);
        payment.RecordAuthoritativeCollectionSuccess(first.Id, Instant.FromUtc(2026, 8, 18, 6, 40));

        Assert.Equal(1, payment.Attempts.Count(a => a.Status == PaymentAttemptStatus.Succeeded));
        Assert.Throws<InvalidOperationException>(() => payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 6, 41)));
    }

    [Fact]
    public void Active_Attempt_Blocks_A_Second_NonTerminal_Attempt()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.CreateAttempt(Now);
        Assert.Throws<InvalidOperationException>(() => payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 6, 50)));
        Assert.Single(payment.Attempts);
    }
}
