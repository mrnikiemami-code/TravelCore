namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// B2B commercial-layer reference to a Party-owned agency. Boundary only — no aggregate, persistence, or Party data ownership.
/// </summary>
public sealed class AgencyReference
{
    private AgencyReference()
    {
        PartyAgencyId = default;
    }

    private AgencyReference(AgencyReferenceId partyAgencyId)
    {
        if (partyAgencyId.Value == Guid.Empty)
        {
            throw new ArgumentException("Party agency reference cannot be empty.", nameof(partyAgencyId));
        }

        PartyAgencyId = partyAgencyId;
    }

    /// <summary>
    /// Logical identifier of the Party-owned agency specialization (AgencyParty.PartyId at runtime).
    /// </summary>
    public AgencyReferenceId PartyAgencyId { get; private set; }

    public static AgencyReference FromPartyAgency(AgencyReferenceId partyAgencyId) =>
        new(partyAgencyId);
}
