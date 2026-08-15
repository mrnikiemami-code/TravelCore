using System.ComponentModel.DataAnnotations;

namespace TravelCore.Modules.Identity.Contracts;

/// <summary>
/// Link or replace Identity→Party association (Identity-owned command).
/// </summary>
public sealed class SetAccountPartyAssociationRequest
{
    [Required]
    public Guid PartyId { get; set; }
}
