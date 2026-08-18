using NodaTime;

namespace TravelCore.Modules.Flight.Contracts;

public enum FlightOfferAvailabilityOutcome : short
{
    Available = 1,
    Unavailable = 2,
    Changed = 3,
    Unknown = 4,
}

public sealed class FlightOfferAvailabilityRequest
{
    public FlightOfferAvailabilityRequest(
        FlightSourceKey sourceKey,
        string sourceOptionReference,
        FlightPassengerCount passengers)
    {
        if (string.IsNullOrWhiteSpace(sourceOptionReference))
        {
            throw new ArgumentException("SourceOptionReference is required.", nameof(sourceOptionReference));
        }

        var reference = sourceOptionReference.Trim();
        if (reference.Length > FlightSearchOption.SourceOptionReferenceMaxLength)
        {
            throw new ArgumentException(
                $"SourceOptionReference max length is {FlightSearchOption.SourceOptionReferenceMaxLength}.",
                nameof(sourceOptionReference));
        }

        ArgumentNullException.ThrowIfNull(passengers);
        SourceKey = sourceKey;
        SourceOptionReference = reference;
        Passengers = passengers;
    }

    public FlightSourceKey SourceKey { get; }

    public string SourceOptionReference { get; }

    public FlightPassengerCount Passengers { get; }
}

public sealed class FlightOfferAvailabilityResult
{
    public FlightOfferAvailabilityResult(
        FlightOfferAvailabilityOutcome outcome,
        FlightSourceKey sourceKey,
        string sourceOptionReference,
        Instant observedAt,
        Instant? expiresAt = null,
        FlightSearchOption? currentOption = null)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Availability outcome is not controlled.");
        }

        if (string.IsNullOrWhiteSpace(sourceOptionReference))
        {
            throw new ArgumentException("SourceOptionReference is required.", nameof(sourceOptionReference));
        }

        if (currentOption is not null && currentOption.SourceKey.Value != sourceKey.Value)
        {
            throw new ArgumentException("Changed option must stay on the same SourceKey.", nameof(currentOption));
        }

        Outcome = outcome;
        SourceKey = sourceKey;
        SourceOptionReference = sourceOptionReference.Trim();
        ObservedAt = observedAt;
        ExpiresAt = expiresAt;
        CurrentOption = currentOption;
    }

    public FlightOfferAvailabilityOutcome Outcome { get; }

    public FlightSourceKey SourceKey { get; }

    public string SourceOptionReference { get; }

    public Instant ObservedAt { get; }

    public Instant? ExpiresAt { get; }

    public FlightSearchOption? CurrentOption { get; }
}
