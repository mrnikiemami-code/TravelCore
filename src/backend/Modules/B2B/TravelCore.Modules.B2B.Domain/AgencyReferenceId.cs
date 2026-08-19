using TravelCore.Identifiers;

namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to a Party-owned agency specialization. B2B does not own Party identity data.
/// </summary>
public readonly record struct AgencyReferenceId(Guid Value)
{
    public static AgencyReferenceId New() => new(Uuid7.New());

    public static AgencyReferenceId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AgencyReferenceId cannot be empty.", nameof(value));
        }

        return new AgencyReferenceId(value);
    }

    public override string ToString() => Value.ToString("D");
}
