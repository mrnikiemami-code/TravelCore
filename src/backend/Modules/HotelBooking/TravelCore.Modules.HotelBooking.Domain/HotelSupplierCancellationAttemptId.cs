using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Strongly typed HotelSupplierCancellationAttempt identity (UUID v7).
/// </summary>
public readonly record struct HotelSupplierCancellationAttemptId(Guid Value)
{
    public static HotelSupplierCancellationAttemptId New() => new(Uuid7.New());

    public static HotelSupplierCancellationAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelSupplierCancellationAttemptId cannot be empty.", nameof(value));
        }

        return new HotelSupplierCancellationAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
