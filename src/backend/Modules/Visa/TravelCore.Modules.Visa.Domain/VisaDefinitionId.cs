using TravelCore.Identifiers;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Strongly typed VisaDefinition identity (UUID v7).
/// </summary>
public readonly record struct VisaDefinitionId(Guid Value) : IEquatable<VisaDefinitionId>
{
    public static VisaDefinitionId New() => new(Uuid7.New());

    public static VisaDefinitionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("VisaDefinitionId cannot be empty.", nameof(value));
        }

        return new VisaDefinitionId(value);
    }

    public override string ToString() => Value.ToString("D");
}
