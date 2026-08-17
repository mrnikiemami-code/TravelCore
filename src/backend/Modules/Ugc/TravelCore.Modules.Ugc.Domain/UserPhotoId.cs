using TravelCore.Identifiers;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Strongly typed UserPhoto identity (UUID v7).
/// </summary>
public readonly record struct UserPhotoId(Guid Value) : IEquatable<UserPhotoId>
{
    public static UserPhotoId New() => new(Uuid7.New());

    public static UserPhotoId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("UserPhotoId cannot be empty.", nameof(value));
        }

        return new UserPhotoId(value);
    }

    public override string ToString() => Value.ToString("D");
}
