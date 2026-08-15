namespace TravelCore.Modules.Party.Contracts;

/// <summary>
/// Minimal Party read probe for presentation gates (kind/display only).
/// </summary>
public interface IPartyReadQuery
{
    Task<PartyReadInfo?> GetAsync(Guid partyId, CancellationToken cancellationToken = default);
}

public sealed class PartyReadInfo
{
    public required Guid Id { get; init; }

    public required string Kind { get; init; }

    public required string DisplayName { get; init; }
}
