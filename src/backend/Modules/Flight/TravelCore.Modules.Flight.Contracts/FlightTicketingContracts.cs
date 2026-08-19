using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Contracts;

public enum FlightTicketingSourceStatus : short
{
    Complete = 1,
    Partial = 2,
    Failed = 3,
    Timeout = 4,
    Unknown = 5,
    NotCreated = 6,
}

public sealed class FlightIssuedTicketFact
{
    public FlightIssuedTicketFact(string givenName, string familyName, string sourceTicketNumber)
    {
        if (string.IsNullOrWhiteSpace(givenName))
        {
            throw new ArgumentException("GivenName is required.", nameof(givenName));
        }

        if (string.IsNullOrWhiteSpace(familyName))
        {
            throw new ArgumentException("FamilyName is required.", nameof(familyName));
        }

        if (string.IsNullOrWhiteSpace(sourceTicketNumber))
        {
            throw new ArgumentException("Source ticket number is required.", nameof(sourceTicketNumber));
        }

        GivenName = givenName.Trim();
        FamilyName = familyName.Trim();
        SourceTicketNumber = sourceTicketNumber.Trim();
    }

    public string GivenName { get; }

    public string FamilyName { get; }

    public string SourceTicketNumber { get; }
}

public sealed class FlightTicketingRequest
{
    public FlightTicketingRequest(
        Guid flightBookingId,
        string sourceKey,
        string sourceReservationReference,
        string reservationLocator,
        IReadOnlyList<FlightReservationPassengerFact> passengers,
        Guid offerSnapshotId,
        MoneyValue total,
        string idempotencyKey)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId is required.", nameof(flightBookingId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReservationReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(reservationLocator);
        ArgumentNullException.ThrowIfNull(passengers);
        ArgumentNullException.ThrowIfNull(total);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        if (passengers.Count == 0)
        {
            throw new ArgumentException("Ticketing requires passengers.", nameof(passengers));
        }

        FlightBookingId = flightBookingId;
        SourceKey = sourceKey.Trim().ToLowerInvariant();
        SourceReservationReference = sourceReservationReference.Trim();
        ReservationLocator = reservationLocator.Trim();
        Passengers = passengers;
        OfferSnapshotId = offerSnapshotId;
        Total = total;
        IdempotencyKey = idempotencyKey.Trim();
    }

    public Guid FlightBookingId { get; }

    public string SourceKey { get; }

    public string SourceReservationReference { get; }

    public string ReservationLocator { get; }

    public IReadOnlyList<FlightReservationPassengerFact> Passengers { get; }

    public Guid OfferSnapshotId { get; }

    public MoneyValue Total { get; }

    public string IdempotencyKey { get; }
}

public sealed class FlightTicketingSourceResult
{
    public FlightTicketingSourceResult(
        FlightTicketingSourceStatus status,
        IReadOnlyList<FlightIssuedTicketFact>? tickets = null,
        string? sourceTicketingReference = null)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Ticketing source status is not controlled.");
        }

        Status = status;
        Tickets = tickets ?? Array.Empty<FlightIssuedTicketFact>();
        SourceTicketingReference = string.IsNullOrWhiteSpace(sourceTicketingReference)
            ? null
            : sourceTicketingReference.Trim();
    }

    public FlightTicketingSourceStatus Status { get; }

    public IReadOnlyList<FlightIssuedTicketFact> Tickets { get; }

    public string? SourceTicketingReference { get; }
}

public sealed class FlightTicketingQueryResult
{
    public FlightTicketingQueryResult(
        FlightTicketingSourceStatus status,
        IReadOnlyList<FlightIssuedTicketFact>? tickets = null,
        bool notFoundProvesNoTicket = false)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Ticketing query status is not controlled.");
        }

        Status = status;
        Tickets = tickets ?? Array.Empty<FlightIssuedTicketFact>();
        NotFoundProvesNoTicket = notFoundProvesNoTicket;
    }

    public FlightTicketingSourceStatus Status { get; }

    public IReadOnlyList<FlightIssuedTicketFact> Tickets { get; }

    public bool NotFoundProvesNoTicket { get; }
}

/// <summary>
/// Provider-neutral Flight ticketing port. Must be the same source that owns the confirmed reservation.
/// Production ticketing source remains NONE.
/// </summary>
public interface IFlightTicketingSource
{
    FlightSourceKey Key { get; }

    IReadOnlySet<FlightSourceCapability> Capabilities { get; }

    /// <summary>
    /// When true, a <see cref="FlightTicketingSourceStatus.NotCreated"/> result
    /// proves no ticket was issued and may mark the attempt Failed.
    /// </summary>
    bool NotFoundProvesNoTicket { get; }

    Task<FlightTicketingSourceResult> CreateTicketsAsync(
        FlightTicketingRequest request,
        CancellationToken cancellationToken = default);

    Task<FlightTicketingQueryResult> QueryTicketStatusAsync(
        string sourceReservationReference,
        CancellationToken cancellationToken = default);
}

public interface IFlightTicketingSourceResolver
{
    IFlightTicketingSource? Resolve(FlightSourceKey sourceKey);

    IReadOnlyList<FlightSourceKey> ListConfiguredKeys();
}

public static class FlightTicketingOwnershipBoundary
{
    public const string TicketingAuthority = "FlightTicketingSource";
    public const string NamedFlightSupplier = "NONE";
    public const string ProductionFlightTicketingSource = "NONE";
    public const string SourcePortName = "IFlightTicketingSource";
    public const string TicketIsNotPnr = "FlightTicket != ReservationLocator";
    public const string TicketIsNotBooking = "FlightTicket != FlightBooking";
    public const string TimeoutIsNotFailed = "NetworkTimeout != FlightTicketingAttempt.Failed";
    public const string TicketStatuses = "Pending, Issued";
    public const string AttemptStatuses = "Created, Initiated, Succeeded, Failed";
    public const string Capabilities = "TicketCreate, TicketQuery";
    public const bool ProductionFakeTicketingSourceImplemented = false;
    public const bool NamedSupplierSdkImplemented = false;
    public const bool CrossSupplierTicketingAllowed = false;
    public const bool VoidImplemented = false;
    public const bool TicketRefundImplemented = false;
    public const bool PublicTicketingApiImplemented = false;
}
