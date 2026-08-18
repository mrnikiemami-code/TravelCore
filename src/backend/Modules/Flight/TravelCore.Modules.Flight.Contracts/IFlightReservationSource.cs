namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Provider-neutral final Flight reservation port. Named production source remains NONE.
/// T005 exposes Create + Query only; cancel/ticket/refund stay out of scope.
/// </summary>
public interface IFlightReservationSource
{
    FlightSourceKey Key { get; }

    IReadOnlySet<FlightSourceCapability> Capabilities { get; }

    /// <summary>
    /// When true, a <see cref="FlightReservationQueryStatus.NotCreated"/> result
    /// proves no PNR/reservation exists and may mark the attempt Failed.
    /// </summary>
    bool NotFoundProvesNoReservation { get; }

    Task<FlightReservationSourceResult> CreateReservationAsync(
        FlightReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<FlightReservationQueryResult> QueryReservationStatusAsync(
        string sourceReservationReference,
        CancellationToken cancellationToken = default);
}
