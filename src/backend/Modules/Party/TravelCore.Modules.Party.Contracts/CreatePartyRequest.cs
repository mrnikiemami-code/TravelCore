using System.ComponentModel.DataAnnotations;

namespace TravelCore.Modules.Party.Contracts;

/// <summary>
/// Create Party command contract (API ≠ domain entity).
/// </summary>
public sealed class CreatePartyRequest
{
    /// <summary>
    /// One of: Person, Organization, Agency (case-insensitive).
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string Kind { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(256)]
    [EmailAddress]
    public string? PrimaryEmail { get; set; }

    [MaxLength(256)]
    public string? PrimaryPhone { get; set; }

    // Person
    [MaxLength(100)]
    public string? GivenName { get; set; }

    [MaxLength(100)]
    public string? FamilyName { get; set; }

    // Organization
    [MaxLength(200)]
    public string? LegalName { get; set; }

    [MaxLength(200)]
    public string? TradeName { get; set; }

    // Agency
    [MaxLength(200)]
    public string? TradingName { get; set; }

    [MaxLength(64)]
    public string? LicenseCode { get; set; }
}
