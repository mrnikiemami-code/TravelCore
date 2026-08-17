namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Structured applicability facts for exactly one VisaRequirementSet (TC-P17-T003 / P17-R3).
/// Logical Destination/country references only. Not a Country/Destination SoT. Not a rules engine.
/// </summary>
public sealed class VisaApplicability
{
    public const int CountryCodeMaxLength = 2;

    private VisaApplicability()
    {
    }

    private VisaApplicability(
        VisaRequirementSetId visaRequirementSetId,
        Guid destinationGeographicId,
        string? applicantNationalityCode,
        string? residenceCountryCode,
        VisaApplicantCategory? applicantCategory)
    {
        if (visaRequirementSetId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(visaRequirementSetId));
        }

        if (destinationGeographicId == Guid.Empty)
        {
            throw new ArgumentException(
                "Destination geographic id cannot be empty.",
                nameof(destinationGeographicId));
        }

        VisaRequirementSetId = visaRequirementSetId;
        DestinationGeographicId = destinationGeographicId;
        ApplicantNationalityCode = applicantNationalityCode;
        ResidenceCountryCode = residenceCountryCode;
        ApplicantCategory = applicantCategory;
    }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    /// <summary>Opaque Destination/jurisdiction logical id. Not a Destination entity or peer FK.</summary>
    public Guid DestinationGeographicId { get; private set; }

    /// <summary>Opaque ISO 3166-1 alpha-2 nationality hint. ReferenceData remains country SoT.</summary>
    public string? ApplicantNationalityCode { get; private set; }

    /// <summary>Opaque ISO 3166-1 alpha-2 residence hint. Not a Country entity or peer FK.</summary>
    public string? ResidenceCountryCode { get; private set; }

    public VisaApplicantCategory? ApplicantCategory { get; private set; }

    internal static VisaApplicability Create(
        VisaRequirementSetId visaRequirementSetId,
        Guid destinationGeographicId,
        string? applicantNationalityCode,
        string? residenceCountryCode,
        string? applicantCategory) =>
        new(
            visaRequirementSetId,
            destinationGeographicId,
            NormalizeCountryCode(applicantNationalityCode, nameof(applicantNationalityCode)),
            NormalizeCountryCode(residenceCountryCode, nameof(residenceCountryCode)),
            VisaApplicantCategory.ParseOptional(applicantCategory));

    public static string? NormalizeCountryCode(string? countryCode, string paramName)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        var trimmed = countryCode.Trim().ToUpperInvariant();
        if (trimmed.Length != CountryCodeMaxLength
            || !trimmed.All(static c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException(
                "Country/nationality code must be ISO 3166-1 alpha-2 (two letters).",
                paramName);
        }

        return trimmed;
    }
}
