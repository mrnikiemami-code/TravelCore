using TravelCore.Identifiers;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Strongly typed VisaRequirementSet identity (UUID v7).
/// </summary>
public readonly record struct VisaRequirementSetId(Guid Value) : IEquatable<VisaRequirementSetId>
{
    public static VisaRequirementSetId New() => new(Uuid7.New());

    public static VisaRequirementSetId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(value));
        }

        return new VisaRequirementSetId(value);
    }

    public override string ToString() => Value.ToString("D");
}
