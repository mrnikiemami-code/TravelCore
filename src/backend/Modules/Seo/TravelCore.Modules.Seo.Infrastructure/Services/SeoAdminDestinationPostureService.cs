using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Job-oriented Destination SEO posture read model for Admin (TC-P05-T011).
/// </summary>
public sealed class SeoAdminDestinationPostureService : ISeoAdminDestinationPostureService
{
    private readonly ISeoRouteService _routes;
    private readonly ISeoIndexPolicyService _policies;

    public SeoAdminDestinationPostureService(
        ISeoRouteService routes,
        ISeoIndexPolicyService policies)
    {
        _routes = routes;
        _policies = policies;
    }

    public async Task<SeoDestinationPostureResponse> GetDestinationPostureAsync(
        Guid destinationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var resourceType = nameof(SeoResourceType.Destination);

        var routes = await _routes.ListByResourceAsync(resourceType, destinationId, cancellationToken);
        var localeRoutes = routes
            .Where(r => string.Equals(r.Locale, normalizedLocale, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var configured = await _policies.GetAsync(
            resourceType,
            destinationId,
            normalizedLocale,
            cancellationToken);

        SeoIndexabilityResponse? effective = null;
        var routeForLocale = localeRoutes.FirstOrDefault();
        if (routeForLocale is not null)
        {
            effective = await _policies.EvaluatePathAsync(
                routeForLocale.Locale,
                routeForLocale.Path,
                cancellationToken);
        }

        var notes = configured is null
            ? "Missing IndexPolicy defaults to noindex,follow (R2). Publish != Index."
            : "Configured IndexPolicy is evaluated against live route eligibility; Index requires eligibility.";

        return new SeoDestinationPostureResponse(
            destinationId,
            normalizedLocale,
            localeRoutes,
            configured,
            effective,
            notes);
    }
}
