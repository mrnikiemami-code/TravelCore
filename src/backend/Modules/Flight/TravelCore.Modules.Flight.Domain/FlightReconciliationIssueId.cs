using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightReconciliationIssueId(Guid Value)
{
    public static FlightReconciliationIssueId New() => new(Uuid7.New());

    public static FlightReconciliationIssueId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightReconciliationIssueId cannot be empty.", nameof(value));
        }

        return new FlightReconciliationIssueId(value);
    }

    public override string ToString() => Value.ToString("D");
}
