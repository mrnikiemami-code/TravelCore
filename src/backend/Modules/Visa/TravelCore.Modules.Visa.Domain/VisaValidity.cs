namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// How long an issued visa remains valid (TC-P17-T005 / P17-R5).
/// Not processing time, allowed stay, entry policy, or a Duration field.
/// </summary>
public sealed class VisaValidity
{
    private VisaValidity()
    {
        Unit = null!;
    }

    private VisaValidity(VisaRequirementSetId visaRequirementSetId, int value, VisaTimeUnit unit)
    {
        VisaRequirementSetId = visaRequirementSetId;
        Value = value;
        Unit = unit;
    }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public int Value { get; private set; }

    public VisaTimeUnit Unit { get; private set; }

    internal static VisaValidity Create(VisaRequirementSetId visaRequirementSetId, int value, string unit)
    {
        if (visaRequirementSetId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(visaRequirementSetId));
        }

        if (value < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Visa validity value must be at least 1.");
        }

        return new VisaValidity(visaRequirementSetId, value, VisaTimeUnit.Parse(unit));
    }
}
