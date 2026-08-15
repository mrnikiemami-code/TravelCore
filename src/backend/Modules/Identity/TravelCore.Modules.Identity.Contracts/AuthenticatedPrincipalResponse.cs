using NodaTime;

namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Minimal authenticated principal projection. No password hash, no Access roles/permissions.
/// </summary>
public sealed class AuthenticatedPrincipalResponse
{
    public required Guid AccountId { get; init; }

    public required string Email { get; init; }

    public required string Status { get; init; }

    public Guid? AssociatedPartyId { get; init; }

    public required Instant AuthenticatedAt { get; init; }
}
