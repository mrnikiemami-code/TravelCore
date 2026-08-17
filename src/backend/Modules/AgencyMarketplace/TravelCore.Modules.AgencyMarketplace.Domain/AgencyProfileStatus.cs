namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Marketplace commercial profile lifecycle. Not Party identity status.
/// </summary>
public enum AgencyProfileStatus : short
{
    Draft = 1,
    Active = 2,
    Archived = 3
}
