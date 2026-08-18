using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Immutable per-room commercial line for an accepted HotelRateOfferSnapshot.
/// Exactly one line per RoomReservationId. Availability selection is optional correlation, not rate identity.
/// </summary>
public sealed class HotelRoomRateSnapshot
{
    public const int ReferenceMaxLength = 128;
    public const int BoardBasisMaxLength = 64;

    private HotelRoomRateSnapshot()
    {
    }

    internal HotelRoomRateSnapshot(
        HotelRateOfferSnapshotId hotelRateOfferSnapshotId,
        RoomReservationId roomReservationId,
        MoneyValue? amount,
        string? availabilitySelectionReference,
        string? sourceRateReference,
        string? boardBasisCode)
    {
        HotelRateOfferSnapshotId = hotelRateOfferSnapshotId;
        RoomReservationId = roomReservationId;
        Amount = amount;
        AvailabilitySelectionReference = availabilitySelectionReference;
        SourceRateReference = sourceRateReference;
        BoardBasisCode = boardBasisCode;
    }

    public HotelRateOfferSnapshotId HotelRateOfferSnapshotId { get; private set; }

    public RoomReservationId RoomReservationId { get; private set; }

    public MoneyValue? Amount { get; private set; }

    public string? AvailabilitySelectionReference { get; private set; }

    public string? SourceRateReference { get; private set; }

    public string? BoardBasisCode { get; private set; }
}
