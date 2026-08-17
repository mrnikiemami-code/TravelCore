using TravelCore.Identifiers;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Strongly typed official visa-fee identity (UUID v7).
/// </summary>
public readonly record struct VisaOfficialFeeId(Guid Value) : IEquatable<VisaOfficialFeeId>
{
    public static VisaOfficialFeeId New() => new(Uuid7.New());

    public static VisaOfficialFeeId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("VisaOfficialFeeId cannot be empty.", nameof(value));
        }

        return new VisaOfficialFeeId(value);
    }

    public override string ToString() => Value.ToString("D");
}
