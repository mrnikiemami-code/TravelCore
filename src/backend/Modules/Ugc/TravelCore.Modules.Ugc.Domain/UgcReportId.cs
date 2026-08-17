using TravelCore.Identifiers;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Strongly typed UgcReport identity (UUID v7).
/// </summary>
public readonly record struct UgcReportId(Guid Value) : IEquatable<UgcReportId>
{
    public static UgcReportId New() => new(Uuid7.New());

    public static UgcReportId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("UgcReportId cannot be empty.", nameof(value));
        }

        return new UgcReportId(value);
    }

    public override string ToString() => Value.ToString("D");
}
