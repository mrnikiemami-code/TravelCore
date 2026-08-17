namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Approximate issuance/review processing time for one VisaRequirementSet (TC-P17-T005 / P17-R5).
/// Not visa validity, allowed stay, entry policy, or a Duration field.
/// </summary>
public sealed class VisaProcessingTime
{
    private VisaProcessingTime()
    {
        Unit = null!;
    }

    private VisaProcessingTime(
        VisaRequirementSetId visaRequirementSetId,
        int minValue,
        int? maxValue,
        VisaTimeUnit unit)
    {
        VisaRequirementSetId = visaRequirementSetId;
        MinValue = minValue;
        MaxValue = maxValue;
        Unit = unit;
    }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public int MinValue { get; private set; }

    public int? MaxValue { get; private set; }

    public VisaTimeUnit Unit { get; private set; }

    internal static VisaProcessingTime Create(
        VisaRequirementSetId visaRequirementSetId,
        int minValue,
        int? maxValue,
        string unit)
    {
        if (visaRequirementSetId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(visaRequirementSetId));
        }

        if (minValue < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minValue), "Processing min value must be at least 1.");
        }

        if (maxValue is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Processing max value must be at least 1.");
        }

        if (maxValue is int max && max < minValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Processing max cannot be less than min.");
        }

        return new VisaProcessingTime(visaRequirementSetId, minValue, maxValue, VisaTimeUnit.Parse(unit));
    }
}
