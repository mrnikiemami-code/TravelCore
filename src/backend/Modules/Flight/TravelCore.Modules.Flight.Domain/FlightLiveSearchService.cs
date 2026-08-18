using NodaTime;
using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Coordinates provider-neutral Flight search and availability. Transient only; no persistence.
/// </summary>
public sealed class FlightLiveSearchService
{
    private readonly IFlightSearchSourceResolver _searchResolver;
    private readonly IFlightOfferAvailabilitySourceResolver _availabilityResolver;
    private readonly IClock _clock;

    public FlightLiveSearchService(
        IFlightSearchSourceResolver searchResolver,
        IFlightOfferAvailabilitySourceResolver availabilityResolver,
        IClock clock)
    {
        _searchResolver = searchResolver ?? throw new ArgumentNullException(nameof(searchResolver));
        _availabilityResolver = availabilityResolver ?? throw new ArgumentNullException(nameof(availabilityResolver));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<FlightSearchResult> SearchAsync(
        FlightSearchRequest request,
        FlightSourceKey? sourceKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var resolvedKey = ResolveSearchKey(sourceKey);
        if (resolvedKey is null)
        {
            return FlightSearchResult.ZeroSource(_clock.GetCurrentInstant());
        }

        var source = _searchResolver.Resolve(resolvedKey.Value);
        if (source is null || !source.Capabilities.Contains(FlightSourceCapability.Search))
        {
            throw new InvalidOperationException("Unknown or disabled Flight search source.");
        }

        try
        {
            return await source.SearchAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return FlightSearchResult.UnknownTimeout(resolvedKey.Value, _clock.GetCurrentInstant());
        }
    }

    public async Task<FlightOfferAvailabilityResult> CheckAvailabilityAsync(
        FlightOfferAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var source = _availabilityResolver.Resolve(request.SourceKey);
        if (source is null || !source.Capabilities.Contains(FlightSourceCapability.AvailabilityCheck))
        {
            throw new InvalidOperationException("Unknown or disabled Flight availability source.");
        }

        try
        {
            var result = await source.CheckAvailabilityAsync(request, cancellationToken);
            if (result.SourceKey.Value != request.SourceKey.Value)
            {
                throw new InvalidOperationException("Cross-source availability validation is forbidden.");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return new FlightOfferAvailabilityResult(
                FlightOfferAvailabilityOutcome.Unknown,
                request.SourceKey,
                request.SourceOptionReference,
                _clock.GetCurrentInstant());
        }
    }

    private FlightSourceKey? ResolveSearchKey(FlightSourceKey? sourceKey)
    {
        if (sourceKey is { } explicitKey)
        {
            return explicitKey;
        }

        var configured = _searchResolver.ListConfiguredKeys();
        if (configured.Count == 0)
        {
            return null;
        }

        if (configured.Count > 1)
        {
            throw new InvalidOperationException(
                "Flight search source is server-controlled; multiple sources require an explicit SourceKey.");
        }

        return configured[0];
    }
}
