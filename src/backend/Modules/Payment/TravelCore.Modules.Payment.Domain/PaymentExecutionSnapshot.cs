using NodaTime;
namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Immutable payment-owned execution obligation copied from Booking trusted obligation (P20-R5).
/// </summary>
public sealed class PaymentExecutionSnapshot
{
    private PaymentExecutionSnapshot()
    {
        Amount = null!;
    }

    private PaymentExecutionSnapshot(Guid bookingSnapshotId, global::TravelCore.Money.Money amount, Instant capturedAt)
    {
        BookingSnapshotId = bookingSnapshotId;
        Amount = amount;
        CapturedAt = capturedAt;
    }

    public Guid BookingSnapshotId { get; private set; }

    public global::TravelCore.Money.Money Amount { get; private set; }

    public Instant CapturedAt { get; private set; }

    internal static PaymentExecutionSnapshot Create(Guid bookingSnapshotId, global::TravelCore.Money.Money amount, Instant capturedAt)
    {
        if (bookingSnapshotId == Guid.Empty)
        {
            throw new ArgumentException("Booking snapshot id cannot be empty.", nameof(bookingSnapshotId));
        }

        ArgumentNullException.ThrowIfNull(amount);
        if (capturedAt == default)
        {
            throw new ArgumentException("CapturedAt cannot be default.", nameof(capturedAt));
        }

        return new PaymentExecutionSnapshot(bookingSnapshotId, amount, capturedAt);
    }

    internal bool Matches(Guid bookingSnapshotId, global::TravelCore.Money.Money amount) =>
        BookingSnapshotId == bookingSnapshotId && Amount == amount;
}
