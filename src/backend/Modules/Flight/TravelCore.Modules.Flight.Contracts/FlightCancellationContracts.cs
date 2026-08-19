using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Contracts;

public sealed class FlightTicketReversalIdentity
{
    public FlightTicketReversalIdentity(Guid ticketId, Guid passengerId, string sourceTicketNumber)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException("TicketId is required.", nameof(ticketId));
        }

        if (passengerId == Guid.Empty)
        {
            throw new ArgumentException("PassengerId is required.", nameof(passengerId));
        }

        if (string.IsNullOrWhiteSpace(sourceTicketNumber))
        {
            throw new ArgumentException("Source ticket number is required.", nameof(sourceTicketNumber));
        }

        TicketId = ticketId;
        PassengerId = passengerId;
        SourceTicketNumber = sourceTicketNumber.Trim();
    }

    public Guid TicketId { get; }

    public Guid PassengerId { get; }

    public string SourceTicketNumber { get; }
}

public sealed class FlightCancellationQuoteRequest
{
    public FlightCancellationQuoteRequest(
        Guid flightBookingId,
        string sourceKey,
        string sourceReservationReference,
        IReadOnlyList<FlightTicketReversalIdentity> issuedTickets)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId is required.", nameof(flightBookingId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReservationReference);
        ArgumentNullException.ThrowIfNull(issuedTickets);

        FlightBookingId = flightBookingId;
        SourceKey = sourceKey.Trim().ToLowerInvariant();
        SourceReservationReference = sourceReservationReference.Trim();
        IssuedTickets = issuedTickets;
    }

    public Guid FlightBookingId { get; }

    public string SourceKey { get; }

    public string SourceReservationReference { get; }

    public IReadOnlyList<FlightTicketReversalIdentity> IssuedTickets { get; }
}

public enum FlightCancellationQuoteSourceOutcome : short
{
    Complete = 1,
    Failed = 2,
    Timeout = 3,
    Unknown = 4,
}

public sealed class FlightCancellationQuoteResult
{
    public FlightCancellationQuoteResult(
        FlightCancellationQuoteSourceOutcome outcome,
        MoneyValue? penaltyAmount = null,
        bool partialRefundRequired = false,
        FlightSupplierReversalKind? ticketReversalKind = null)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Quote outcome is not controlled.");
        }

        if (ticketReversalKind is { } kind
            && kind is not FlightSupplierReversalKind.TicketVoid
            && kind is not FlightSupplierReversalKind.TicketRefund)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ticketReversalKind),
                ticketReversalKind,
                "Quote ticket reversal kind must be TicketVoid or TicketRefund.");
        }

        Outcome = outcome;
        PenaltyAmount = penaltyAmount;
        PartialRefundRequired = partialRefundRequired;
        TicketReversalKind = ticketReversalKind;
    }

    public FlightCancellationQuoteSourceOutcome Outcome { get; }

    public MoneyValue? PenaltyAmount { get; }

    public bool PartialRefundRequired { get; }

    public FlightSupplierReversalKind? TicketReversalKind { get; }
}

public sealed class FlightReservationCancelRequest
{
    public FlightReservationCancelRequest(
        Guid flightBookingId,
        Guid flightBookingCancellationId,
        string sourceKey,
        string sourceReservationReference,
        string idempotencyKey)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId is required.", nameof(flightBookingId));
        }

        if (flightBookingCancellationId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingCancellationId is required.", nameof(flightBookingCancellationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReservationReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        FlightBookingId = flightBookingId;
        FlightBookingCancellationId = flightBookingCancellationId;
        SourceKey = sourceKey.Trim().ToLowerInvariant();
        SourceReservationReference = sourceReservationReference.Trim();
        IdempotencyKey = idempotencyKey.Trim();
    }

    public Guid FlightBookingId { get; }

    public Guid FlightBookingCancellationId { get; }

    public string SourceKey { get; }

    public string SourceReservationReference { get; }

    public string IdempotencyKey { get; }
}

public enum FlightReservationCancelSourceOutcome : short
{
    Succeeded = 1,
    Failed = 2,
    Timeout = 3,
    Unknown = 4,
}

public sealed class FlightReservationCancelSourceResult
{
    public FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome outcome)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Reservation cancel outcome is not controlled.");
        }

        Outcome = outcome;
    }

    public FlightReservationCancelSourceOutcome Outcome { get; }
}

public sealed class FlightTicketReversalRequest
{
    public FlightTicketReversalRequest(
        Guid flightBookingId,
        Guid flightBookingCancellationId,
        Guid ticketId,
        Guid passengerId,
        string sourceKey,
        string sourceReservationReference,
        string sourceTicketNumber,
        FlightSupplierReversalKind kind,
        string idempotencyKey)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId is required.", nameof(flightBookingId));
        }

        if (flightBookingCancellationId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingCancellationId is required.", nameof(flightBookingCancellationId));
        }

        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException("TicketId is required.", nameof(ticketId));
        }

        if (passengerId == Guid.Empty)
        {
            throw new ArgumentException("PassengerId is required.", nameof(passengerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReservationReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTicketNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (kind is not FlightSupplierReversalKind.TicketVoid and not FlightSupplierReversalKind.TicketRefund)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Ticket reversal kind must be TicketVoid or TicketRefund.");
        }

        FlightBookingId = flightBookingId;
        FlightBookingCancellationId = flightBookingCancellationId;
        TicketId = ticketId;
        PassengerId = passengerId;
        SourceKey = sourceKey.Trim().ToLowerInvariant();
        SourceReservationReference = sourceReservationReference.Trim();
        SourceTicketNumber = sourceTicketNumber.Trim();
        Kind = kind;
        IdempotencyKey = idempotencyKey.Trim();
    }

    public Guid FlightBookingId { get; }

    public Guid FlightBookingCancellationId { get; }

    public Guid TicketId { get; }

    public Guid PassengerId { get; }

    public string SourceKey { get; }

    public string SourceReservationReference { get; }

    public string SourceTicketNumber { get; }

    public FlightSupplierReversalKind Kind { get; }

    public string IdempotencyKey { get; }
}

public enum FlightTicketReversalSourceOutcome : short
{
    Voided = 1,
    Refunded = 2,
    Failed = 3,
    Timeout = 4,
    Unknown = 5,
}

public sealed class FlightTicketReversalSourceResult
{
    public FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome outcome)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Ticket reversal outcome is not controlled.");
        }

        Outcome = outcome;
    }

    public FlightTicketReversalSourceOutcome Outcome { get; }
}

public sealed class FlightCancellationQueryRequest
{
    public FlightCancellationQueryRequest(
        Guid flightBookingId,
        string sourceKey,
        string sourceReservationReference,
        bool sourceVerified)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId is required.", nameof(flightBookingId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReservationReference);

        FlightBookingId = flightBookingId;
        SourceKey = sourceKey.Trim().ToLowerInvariant();
        SourceReservationReference = sourceReservationReference.Trim();
        SourceVerified = sourceVerified;
    }

    public Guid FlightBookingId { get; }

    public string SourceKey { get; }

    public string SourceReservationReference { get; }

    public bool SourceVerified { get; }
}

public enum FlightCancellationQueryStatus : short
{
    Cancelled = 1,
    Active = 2,
    PendingUnknown = 3,
    NotFound = 4,
}

public sealed class FlightCancellationQueryResult
{
    public FlightCancellationQueryResult(FlightCancellationQueryStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Cancellation query status is not controlled.");
        }

        Status = status;
    }

    public FlightCancellationQueryStatus Status { get; }
}

public sealed class FlightTicketReversalQueryRequest
{
    public FlightTicketReversalQueryRequest(
        Guid ticketId,
        Guid passengerId,
        string sourceKey,
        string sourceTicketNumber,
        bool sourceVerified)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException("TicketId is required.", nameof(ticketId));
        }

        if (passengerId == Guid.Empty)
        {
            throw new ArgumentException("PassengerId is required.", nameof(passengerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTicketNumber);

        TicketId = ticketId;
        PassengerId = passengerId;
        SourceKey = sourceKey.Trim().ToLowerInvariant();
        SourceTicketNumber = sourceTicketNumber.Trim();
        SourceVerified = sourceVerified;
    }

    public Guid TicketId { get; }

    public Guid PassengerId { get; }

    public string SourceKey { get; }

    public string SourceTicketNumber { get; }

    public bool SourceVerified { get; }
}

public enum FlightTicketReversalQueryStatus : short
{
    Voided = 1,
    Refunded = 2,
    Issued = 3,
    PendingUnknown = 4,
    NotFound = 5,
}

public sealed class FlightTicketReversalQueryResult
{
    public FlightTicketReversalQueryResult(Guid ticketId, FlightTicketReversalQueryStatus status)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException("TicketId is required.", nameof(ticketId));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Ticket reversal query status is not controlled.");
        }

        TicketId = ticketId;
        Status = status;
    }

    public Guid TicketId { get; }

    public FlightTicketReversalQueryStatus Status { get; }
}

/// <summary>
/// Provider-neutral Flight cancellation / ticket reversal port. Named production source remains NONE.
/// Not a giant gateway; reservation create and ticketing stay on their own ports.
/// </summary>
public interface IFlightCancellationSource
{
    FlightSourceKey Key { get; }

    IReadOnlySet<FlightSourceCapability> Capabilities { get; }

    Task<FlightCancellationQuoteResult> QuoteCancellationAsync(
        FlightCancellationQuoteRequest request,
        CancellationToken cancellationToken = default);

    Task<FlightReservationCancelSourceResult> CancelReservationAsync(
        FlightReservationCancelRequest request,
        CancellationToken cancellationToken = default);

    Task<FlightTicketReversalSourceResult> ReverseTicketAsync(
        FlightTicketReversalRequest request,
        CancellationToken cancellationToken = default);

    Task<FlightCancellationQueryResult> QueryCancellationStatusAsync(
        FlightCancellationQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<FlightTicketReversalQueryResult> QueryTicketReversalStatusAsync(
        FlightTicketReversalQueryRequest request,
        CancellationToken cancellationToken = default);
}

public interface IFlightCancellationSourceResolver
{
    IFlightCancellationSource? Resolve(FlightSourceKey sourceKey);

    IReadOnlyList<FlightSourceKey> ListConfiguredKeys();
}
