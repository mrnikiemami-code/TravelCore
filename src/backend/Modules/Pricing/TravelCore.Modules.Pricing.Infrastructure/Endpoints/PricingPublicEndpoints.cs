using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Pricing.Contracts;

namespace TravelCore.Modules.Pricing.Infrastructure.Endpoints;

/// <summary>
/// Public Pricing HTTP surface (TC-P12-T008 / P12-R8). Anonymous read-only price facts.
/// </summary>
internal static class PricingPublicEndpoints
{
    public static IEndpointRouteBuilder MapPricingPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/pricing/public")
            .WithTags("PricingPublic")
            .AllowAnonymous();

        group.MapGet("/summaries", async Task<IResult> (
            string? targetType,
            Guid? targetId,
            IPublicPricingQuery query,
            CancellationToken cancellationToken) =>
        {
            if (targetId is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["targetId"] = ["TargetId is required."]
                });
            }

            try
            {
                var summary = await query.GetSummaryAsync(
                    targetType ?? string.Empty,
                    targetId.Value,
                    cancellationToken);
                return summary is null ? Results.NotFound() : Results.Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapGet("/tour-departures/{tourDepartureId:guid}", async Task<IResult> (
            Guid tourDepartureId,
            IPublicPricingQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var summary = await query.GetByTourDepartureIdAsync(tourDepartureId, cancellationToken);
                return summary is null ? Results.NotFound() : Results.Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
