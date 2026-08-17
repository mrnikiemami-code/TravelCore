namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Logical destination planning preference (P18-R4). Not Destination/Place SoT.
/// </summary>
public sealed class DestinationPreference
{
    private DestinationPreference()
    {
    }

    private DestinationPreference(int sortOrder, Guid? logicalDestinationId, bool isUndecided)
    {
        SortOrder = sortOrder;
        LogicalDestinationId = logicalDestinationId;
        IsUndecided = isUndecided;
    }

    public int SortOrder { get; private set; }

    public Guid? LogicalDestinationId { get; private set; }

    public bool IsUndecided { get; private set; }

    public static DestinationPreference Undecided(int sortOrder = 0) =>
        new(sortOrder, null, true);

    public static DestinationPreference ForLogicalDestination(Guid logicalDestinationId, int sortOrder = 0)
    {
        if (logicalDestinationId == Guid.Empty)
        {
            throw new ArgumentException("Logical destination id cannot be empty.", nameof(logicalDestinationId));
        }

        return new DestinationPreference(sortOrder, logicalDestinationId, false);
    }

    internal DestinationPreference CaptureCopy() =>
        new(SortOrder, LogicalDestinationId, IsUndecided);
}
