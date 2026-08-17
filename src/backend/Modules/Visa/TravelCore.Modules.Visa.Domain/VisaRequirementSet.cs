using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Context-dependent requirement facts for one VisaDefinition (TC-P17-T002 / P17-R2).
/// Owns exactly one VisaApplicability (TC-P17-T003 / P17-R3). Documents, processing, and fees remain later R#.
/// </summary>
public sealed class VisaRequirementSet
{
    private VisaApplicability _applicability = null!;

    private VisaRequirementSet()
    {
    }

    private VisaRequirementSet(VisaRequirementSetId id, VisaDefinitionId visaDefinitionId, Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(id));
        }

        if (visaDefinitionId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaDefinitionId cannot be empty.", nameof(visaDefinitionId));
        }

        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        Id = id;
        VisaDefinitionId = visaDefinitionId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public VisaRequirementSetId Id { get; private set; }

    /// <summary>Owning VisaDefinition. Same-schema relationship only.</summary>
    public VisaDefinitionId VisaDefinitionId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>Exactly one structured applicability context. Not a rules engine.</summary>
    public VisaApplicability Applicability => _applicability;

    internal static VisaRequirementSet Create(
        VisaRequirementSetId id,
        VisaDefinitionId visaDefinitionId,
        Guid destinationGeographicId,
        Instant now,
        string? applicantNationalityCode = null,
        string? residenceCountryCode = null,
        string? applicantCategory = null)
    {
        var set = new VisaRequirementSet(id, visaDefinitionId, now);
        set._applicability = VisaApplicability.Create(
            id,
            destinationGeographicId,
            applicantNationalityCode,
            residenceCountryCode,
            applicantCategory);
        return set;
    }
}
