using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Per-passenger Flight ticket fact. PNR is not a ticket. Supplier e-ticket number is opaque source fact.
/// </summary>
public sealed class FlightTicket
{
    public const int SourceTicketNumberMaxLength = 64;
    public const int SourceKeyMaxLength = 64;

    private FlightTicket()
    {
        SourceKey = string.Empty;
    }

    private FlightTicket(
        FlightTicketId id,
        FlightBookingId flightBookingId,
        FlightPassengerId passengerId,
        string sourceKey,
        Instant createdAt)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        PassengerId = passengerId;
        SourceKey = sourceKey;
        Status = FlightTicketStatus.Pending;
        CreatedAt = createdAt;
    }

    public FlightTicketId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public FlightPassengerId PassengerId { get; private set; }

    public string SourceKey { get; private set; }

    public FlightTicketStatus Status { get; private set; }

    public string? SourceTicketNumber { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? IssuedAt { get; private set; }

    public Instant? VoidedAt { get; private set; }

    public Instant? RefundedAt { get; private set; }

    public static FlightTicket StartPending(
        FlightBookingId flightBookingId,
        FlightPassengerId passengerId,
        string sourceKey,
        Instant createdAt)
    {
        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("SourceKey is required.", nameof(sourceKey));
        }

        var normalized = sourceKey.Trim().ToLowerInvariant();
        if (normalized.Length > SourceKeyMaxLength)
        {
            throw new ArgumentException($"SourceKey max length is {SourceKeyMaxLength}.", nameof(sourceKey));
        }

        return new FlightTicket(FlightTicketId.New(), flightBookingId, passengerId, normalized, createdAt);
    }

    public void MarkIssued(string sourceTicketNumber, Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("IssuedAt cannot be default.", nameof(now));
        }

        if (string.IsNullOrWhiteSpace(sourceTicketNumber))
        {
            throw new ArgumentException("Source ticket number is required to issue.", nameof(sourceTicketNumber));
        }

        var trimmed = sourceTicketNumber.Trim();
        if (trimmed.Length > SourceTicketNumberMaxLength)
        {
            throw new ArgumentException(
                $"Source ticket number max length is {SourceTicketNumberMaxLength}.",
                nameof(sourceTicketNumber));
        }

        if (Status == FlightTicketStatus.Issued)
        {
            if (!string.Equals(SourceTicketNumber, trimmed, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Issued ticket number cannot be rewritten.");
            }

            return;
        }

        if (Status != FlightTicketStatus.Pending)
        {
            throw new InvalidOperationException($"Ticket in status {Status} cannot become Issued.");
        }

        Status = FlightTicketStatus.Issued;
        SourceTicketNumber = trimmed;
        IssuedAt = now;
    }

    /// <summary>
    /// Supplier-authoritative ticket void. Not Payment Refund. Issued only.
    /// </summary>
    public void MarkVoided(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("VoidedAt cannot be default.", nameof(now));
        }

        if (Status == FlightTicketStatus.Voided)
        {
            return;
        }

        if (Status != FlightTicketStatus.Issued)
        {
            throw new InvalidOperationException($"Ticket in status {Status} cannot become Voided.");
        }

        Status = FlightTicketStatus.Voided;
        VoidedAt = now;
    }

    /// <summary>
    /// Supplier-authoritative airline-side ticket refund/reversal. Not Payment Refund. Issued only.
    /// </summary>
    public void MarkRefunded(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("RefundedAt cannot be default.", nameof(now));
        }

        if (Status == FlightTicketStatus.Refunded)
        {
            return;
        }

        if (Status != FlightTicketStatus.Issued)
        {
            throw new InvalidOperationException($"Ticket in status {Status} cannot become Refunded.");
        }

        Status = FlightTicketStatus.Refunded;
        RefundedAt = now;
    }
}
