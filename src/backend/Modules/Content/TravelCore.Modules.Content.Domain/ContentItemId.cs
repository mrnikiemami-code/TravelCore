using TravelCore.Identifiers;

namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Strongly typed ContentItem identity (UUID v7).
/// Canonical editorial id for Article / LandingPage / Guide (P08-R1) — no independent public subtype ids.
/// </summary>
public readonly record struct ContentItemId(Guid Value) : IEquatable<ContentItemId>
{
    public static ContentItemId New() => new(Uuid7.New());

    public static ContentItemId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ContentItemId cannot be empty.", nameof(value));
        }

        return new ContentItemId(value);
    }

    public override string ToString() => Value.ToString("D");
}
