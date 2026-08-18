using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct HotelBookingReconciliationIssueId(Guid Value)
{
    public static HotelBookingReconciliationIssueId New() => new(Uuid7.New());

    public static HotelBookingReconciliationIssueId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingReconciliationIssueId cannot be empty.", nameof(value));
        }

        return new HotelBookingReconciliationIssueId(value);
    }

    public override string ToString() => Value.ToString("D");
}
