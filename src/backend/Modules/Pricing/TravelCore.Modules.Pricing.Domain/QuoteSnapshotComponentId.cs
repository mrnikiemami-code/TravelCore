using TravelCore.Identifiers;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Strongly typed identity for an immutable <see cref="QuoteSnapshotComponent"/> line (UUID v7).
/// </summary>
public readonly record struct QuoteSnapshotComponentId(Guid Value) : IEquatable<QuoteSnapshotComponentId>
{
    public static QuoteSnapshotComponentId New() => new(Uuid7.New());

    public static QuoteSnapshotComponentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("QuoteSnapshotComponentId cannot be empty.", nameof(value));
        }

        return new QuoteSnapshotComponentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
