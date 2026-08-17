using TravelCore.Identifiers;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Strongly typed Lead identity (UUID v7).
/// </summary>
public readonly record struct LeadId(Guid Value) : IEquatable<LeadId>
{
    public static LeadId New() => new(Uuid7.New());

    public static LeadId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("LeadId cannot be empty.", nameof(value));
        }

        return new LeadId(value);
    }

    public override string ToString() => Value.ToString("D");
}
