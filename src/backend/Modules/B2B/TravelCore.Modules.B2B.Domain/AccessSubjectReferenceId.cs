using TravelCore.Identifiers;

namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to an Access authorization subject. Agency users remain Access-owned subjects.
/// </summary>
public readonly record struct AccessSubjectReferenceId(Guid Value)
{
    public static AccessSubjectReferenceId New() => new(Uuid7.New());

    public static AccessSubjectReferenceId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AccessSubjectReferenceId cannot be empty.", nameof(value));
        }

        return new AccessSubjectReferenceId(value);
    }

    public override string ToString() => Value.ToString("D");
}
