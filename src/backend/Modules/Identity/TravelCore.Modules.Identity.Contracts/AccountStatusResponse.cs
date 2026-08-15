using NodaTime;

namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Account status read contract. Never includes password or hash.
/// </summary>
public sealed class AccountStatusResponse
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Status { get; init; }

    public Guid? AssociatedPartyId { get; init; }

    public required Instant CreatedAt { get; init; }

    public required Instant UpdatedAt { get; init; }
}
