namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Entry count/policy for one VisaRequirementSet (TC-P17-T005 / P17-R5).
/// Not inferred from processing time, validity, or allowed stay.
/// </summary>
public sealed class VisaEntryPolicy
{
    private VisaEntryPolicy()
    {
        Kind = null!;
    }

    private VisaEntryPolicy(VisaRequirementSetId visaRequirementSetId, VisaEntryKind kind)
    {
        VisaRequirementSetId = visaRequirementSetId;
        Kind = kind;
    }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public VisaEntryKind Kind { get; private set; }

    internal static VisaEntryPolicy Create(VisaRequirementSetId visaRequirementSetId, string kind)
    {
        if (visaRequirementSetId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(visaRequirementSetId));
        }

        return new VisaEntryPolicy(visaRequirementSetId, VisaEntryKind.Parse(kind));
    }
}
