using TravelCore.Identifiers;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Strongly typed SeoPathReservation identity (UUID v7).
/// </summary>
public readonly record struct SeoPathReservationId(Guid Value) : IEquatable<SeoPathReservationId>
{
    public static SeoPathReservationId New() => new(Uuid7.New());

    public static SeoPathReservationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SeoPathReservationId cannot be empty.", nameof(value));
        }

        return new SeoPathReservationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
