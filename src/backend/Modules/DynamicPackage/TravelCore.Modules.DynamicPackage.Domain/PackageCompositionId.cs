using TravelCore.Identifiers;

namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Strongly typed PackageComposition identity for DynamicPackage composition boundary.
/// UUID v7.
/// </summary>
public readonly record struct PackageCompositionId(Guid Value)
{
    public static PackageCompositionId New() => new(Uuid7.New());

    public static PackageCompositionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PackageCompositionId cannot be empty.", nameof(value));
        }

        return new PackageCompositionId(value);
    }

    public override string ToString() => Value.ToString("D");
}

