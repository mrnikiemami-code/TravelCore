using System.ComponentModel.DataAnnotations;

namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Login command. Password is accepted only for verification — never returned.
/// </summary>
public sealed class LoginRequest
{
    [Required]
    [MaxLength(320)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
