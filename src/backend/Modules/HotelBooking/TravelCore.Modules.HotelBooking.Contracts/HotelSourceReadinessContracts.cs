namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Explicit Hotel source capability declarations (P21-R8). Capabilities are never inferred from SourceKey.
/// Production named supplier and production sources remain NONE.
/// </summary>
public enum HotelSourceCapability
{
    AvailabilityCheck = 1,
    AvailabilityHold = 2,
    AvailabilityHoldQuery = 3,
    AvailabilityHoldRelease = 4,
    RateQuote = 5,
    ReservationCreate = 6,
    ReservationQuery = 7,
    ReservationCancel = 8,
    ReservationCancellationQuery = 9,
}

public static class HotelSourceReadinessBoundary
{
    public const string NamedHotelSupplier = "NONE";
    public const string ProductionAvailabilitySource = "NONE";
    public const string ProductionRateSource = "NONE";
    public const string ProductionReservationSource = "NONE";
    public const string ProductionPaymentProvider = "NONE";
    public const bool ZeroProductionSourcesValid = true;
    public const bool SmartRoutingImplemented = false;
    public const bool AutomaticFailoverImplemented = false;
    public const bool CapabilityInferredFromSourceName = false;

    public static readonly HotelSourceCapability[] DeclaredCapabilities =
    [
        HotelSourceCapability.AvailabilityCheck,
        HotelSourceCapability.AvailabilityHold,
        HotelSourceCapability.AvailabilityHoldQuery,
        HotelSourceCapability.AvailabilityHoldRelease,
        HotelSourceCapability.RateQuote,
        HotelSourceCapability.ReservationCreate,
        HotelSourceCapability.ReservationQuery,
        HotelSourceCapability.ReservationCancel,
        HotelSourceCapability.ReservationCancellationQuery,
    ];
}

public sealed record HotelSourceDescriptor(
    string SourceKey,
    bool Enabled,
    IReadOnlyList<HotelSourceCapability> Capabilities,
    string? DisplayName);

public interface IDeclaredHotelSourceCapabilities
{
    string SourceKey { get; }

    bool Enabled { get; }

    string? DisplayName { get; }

    IReadOnlyList<HotelSourceCapability> Capabilities { get; }
}

public interface IHotelSourceCatalog
{
    IReadOnlyList<HotelSourceDescriptor> List();

    HotelSourceDescriptor? Find(string sourceKey);
}
