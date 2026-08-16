namespace TravelCore.Modules.Destination.Contracts;

/// <summary>
/// Cross-module Destination existence probe for Place DestinationId association (no EF leakage).
/// Logical identity only — consumers must not introduce cross-schema FKs.
/// </summary>
public interface IDestinationExistenceQuery
{
    Task<bool> ExistsAsync(Guid destinationId, CancellationToken cancellationToken = default);
}
