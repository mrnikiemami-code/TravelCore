namespace TravelCore.Modules.Destination.Domain;

/// <summary>
/// Closed P04 DestinationKind classification (architect R1).
/// Not Place/Hotel/Airport/POI/Tour/Content/Media.
/// </summary>
public enum DestinationKind : short
{
    Country = 1,
    Region = 2,
    City = 3,
    Area = 4
}
