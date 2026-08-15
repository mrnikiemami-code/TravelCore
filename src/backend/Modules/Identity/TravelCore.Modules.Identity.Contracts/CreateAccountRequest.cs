using System.ComponentModel.DataAnnotations;

namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Create Account command. Password is accepted only for hashing — never returned.
/// </summary>
public sealed class CreateAccountRequest
{
    [Required]
    [MaxLength(320)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional opaque Party id reference (no FK, no Party profile). Association workflow is T004.
    /// </summary>
    public Guid? AssociatedPartyId { get; set; }
}
