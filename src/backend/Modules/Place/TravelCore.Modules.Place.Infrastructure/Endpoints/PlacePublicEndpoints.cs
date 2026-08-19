using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Place.Contracts;

namespace TravelCore.Modules.Place.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public Place browse reads (TC-HOTIDX-T003).
/// Active catalog discovery — not admin mutations · not Search engine.
/// </summary>
internal static class PlacePublicEndpoints
{
    public static IEndpointRouteBuilder MapPlacePublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicPlace = endpoints.MapGroup("/api/place/public")
            .WithTags("Place");

        publicPlace.MapGet("/hotels", async Task<IResult> (
            string localeCode,
            int? take,
            IPlacePublicHotelBrowseQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.ListByLocaleAsync(
                    localeCode,
                    take ?? PlacePublicBrowseLimits.MaxPublicHotels,
                    cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
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
            [ex.ParamName ?? "value"] = [ex.Message]
        });
}
