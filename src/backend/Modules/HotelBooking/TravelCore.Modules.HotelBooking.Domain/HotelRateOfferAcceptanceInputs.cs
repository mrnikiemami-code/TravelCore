using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

public sealed record HotelRoomRateLine(
    RoomReservationId RoomReservationId,
    MoneyValue? Amount = null,
    string? AvailabilitySelectionReference = null,
    string? SourceRateReference = null,
    string? BoardBasisCode = null);

public sealed record HotelCancellationPenaltyRuleDraft(
    Instant EffectiveFrom,
    Instant? EffectiveUntil,
    MoneyValue Penalty);

public sealed record HotelChargeComponentLine(
    string Code,
    MoneyValue Amount);
