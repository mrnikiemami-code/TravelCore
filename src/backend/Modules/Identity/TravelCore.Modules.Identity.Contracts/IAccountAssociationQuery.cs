namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Reads Identity→Party association for presentation/acting-as gates.
/// </summary>
public interface IAccountAssociationQuery
{
    Task<Guid?> GetAssociatedPartyIdAsync(Guid accountId, CancellationToken cancellationToken = default);
}
