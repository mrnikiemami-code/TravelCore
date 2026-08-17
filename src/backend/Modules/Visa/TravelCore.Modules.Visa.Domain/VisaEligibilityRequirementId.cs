using TravelCore.Identifiers;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Strongly typed eligibility-requirement identity (UUID v7).
/// </summary>
public readonly record struct VisaEligibilityRequirementId(Guid Value) : IEquatable<VisaEligibilityRequirementId>
{
    public static VisaEligibilityRequirementId New() => new(Uuid7.New());

    public static VisaEligibilityRequirementId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("VisaEligibilityRequirementId cannot be empty.", nameof(value));
        }

        return new VisaEligibilityRequirementId(value);
    }

    public override string ToString() => Value.ToString("D");
}
