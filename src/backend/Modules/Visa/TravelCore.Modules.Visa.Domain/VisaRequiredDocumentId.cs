using TravelCore.Identifiers;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Strongly typed required-document identity (UUID v7).
/// </summary>
public readonly record struct VisaRequiredDocumentId(Guid Value) : IEquatable<VisaRequiredDocumentId>
{
    public static VisaRequiredDocumentId New() => new(Uuid7.New());

    public static VisaRequiredDocumentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequiredDocumentId cannot be empty.", nameof(value));
        }

        return new VisaRequiredDocumentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
