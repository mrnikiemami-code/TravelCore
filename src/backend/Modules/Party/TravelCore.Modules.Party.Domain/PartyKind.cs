namespace TravelCore.Modules.Party.Domain;

/// <summary>
/// Business identity specialization for a Party.
/// </summary>
public enum PartyKind : short
{
    Person = 1,
    Organization = 2,
    Agency = 3
}
