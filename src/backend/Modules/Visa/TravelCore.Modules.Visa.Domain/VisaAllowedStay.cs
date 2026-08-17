namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Maximum allowed presence in the destination (TC-P17-T005 / P17-R5).
/// Not processing time, visa validity, entry policy, or a Duration field.
/// </summary>
public sealed class VisaAllowedStay
{
    private VisaAllowedStay()
    {
        Unit = null!;
    }

    private VisaAllowedStay(VisaRequirementSetId visaRequirementSetId, int value, VisaTimeUnit unit)
    {
        VisaRequirementSetId = visaRequirementSetId;
        Value = value;
        Unit = unit;
    }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public int Value { get; private set; }

    public VisaTimeUnit Unit { get; private set; }

    internal static VisaAllowedStay Create(VisaRequirementSetId visaRequirementSetId, int value, string unit)
    {
        if (visaRequirementSetId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(visaRequirementSetId));
        }

        if (value < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Allowed stay value must be at least 1.");
        }

        return new VisaAllowedStay(visaRequirementSetId, value, VisaTimeUnit.Parse(unit));
    }
}
