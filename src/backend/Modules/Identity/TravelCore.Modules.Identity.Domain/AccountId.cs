using TravelCore.Identifiers;

namespace TravelCore.Modules.Identity.Domain;

/// <summary>
/// Strongly typed Identity Account id (UUID v7).
/// </summary>
public readonly record struct AccountId(Guid Value)
{
    public static AccountId New() => new(Uuid7.New());

    public static AccountId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AccountId cannot be empty.", nameof(value));
        }

        return new AccountId(value);
    }

    public override string ToString() => Value.ToString("D");
}
