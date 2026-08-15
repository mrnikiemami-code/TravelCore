namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Person specialization owned by the Party aggregate (same identity as Party root).
/// </summary>
public sealed class PersonParty
{
    private PersonParty()
    {
        GivenName = null!;
        FamilyName = null!;
    }

    internal PersonParty(PartyId partyId, string givenName, string familyName)
    {
        PartyId = partyId;
        GivenName = NormalizeRequired(givenName, nameof(givenName), 100);
        FamilyName = NormalizeRequired(familyName, nameof(familyName), 100);
    }

    public PartyId PartyId { get; private set; }

    public string GivenName { get; private set; }

    public string FamilyName { get; private set; }

    private static string NormalizeRequired(string value, string paramName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Length must be <= {maxLength}.");
        }

        return trimmed;
    }
}
