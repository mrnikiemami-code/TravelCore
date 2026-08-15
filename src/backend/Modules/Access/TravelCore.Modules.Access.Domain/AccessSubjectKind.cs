namespace TravelCore.Modules.Access.Domain;

/// <summary>
/// Subject kind for Access assignments. v1 supports Identity and Party subjects
/// (R3 default: Identity-centric primary; Party allowed when Party exists).
/// </summary>
public enum AccessSubjectKind : short
{
    Identity = 1,
    Party = 2
}
