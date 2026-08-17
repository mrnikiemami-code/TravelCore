using TravelCore.Identifiers;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Strongly typed Comment identity (UUID v7).
/// </summary>
public readonly record struct CommentId(Guid Value) : IEquatable<CommentId>
{
    public static CommentId New() => new(Uuid7.New());

    public static CommentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommentId cannot be empty.", nameof(value));
        }

        return new CommentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
