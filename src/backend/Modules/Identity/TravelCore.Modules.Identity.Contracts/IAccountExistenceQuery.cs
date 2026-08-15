namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Cross-module Identity account existence probe for Access subject assignment.
/// </summary>
public interface IAccountExistenceQuery
{
    Task<bool> ExistsAsync(Guid accountId, CancellationToken cancellationToken = default);
}
