namespace TravelCore.Modules.Party.Contracts;

/// <summary>
/// Cross-module Party existence probe for Identity association (no EF leakage).
/// </summary>
public interface IPartyExistenceQuery
{
    Task<bool> ExistsAsync(Guid partyId, CancellationToken cancellationToken = default);
}
