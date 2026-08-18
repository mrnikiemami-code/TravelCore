namespace TravelCore.Modules.HotelBooking.Domain;

public sealed class HotelAvailabilityHoldRoom
{
    public const int SelectionMaxLength = 128;

    private HotelAvailabilityHoldRoom()
    {
    }

    internal HotelAvailabilityHoldRoom(
        HotelAvailabilityHoldId holdId,
        RoomReservationId roomReservationId)
    {
        HoldId = holdId;
        RoomReservationId = roomReservationId;
    }

    public HotelAvailabilityHoldId HoldId { get; private set; }

    public RoomReservationId RoomReservationId { get; private set; }

    public string? SelectionReference { get; private set; }

    internal void AssignSelection(string selectionReference)
    {
        if (string.IsNullOrWhiteSpace(selectionReference))
        {
            throw new ArgumentException("Selection reference is required.", nameof(selectionReference));
        }

        var trimmed = selectionReference.Trim();
        if (trimmed.Length > SelectionMaxLength)
        {
            throw new ArgumentException($"Selection max length is {SelectionMaxLength}.", nameof(selectionReference));
        }

        SelectionReference = trimmed;
    }
}
