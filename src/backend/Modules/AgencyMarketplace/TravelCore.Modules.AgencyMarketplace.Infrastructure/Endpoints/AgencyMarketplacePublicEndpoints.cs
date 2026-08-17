using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.AgencyMarketplace.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public AgencyOffer read (TC-P14-T007 / P14-R7). Presentation facts only — not Booking.
/// </summary>
internal static class AgencyMarketplacePublicEndpoints
{
    public static IEndpointRouteBuilder MapAgencyMarketplacePublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var offers = endpoints.MapGroup("/api/agency-marketplace/offers")
            .WithTags("AgencyMarketplace");

        offers.MapGet("/related-published", async Task<IResult> (
            Guid tourProductId,
            IRelatedAgencyOfferPublicQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var related = await query.GetByTourProductAsync(tourProductId, cancellationToken);
                return Results.Ok(related);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "tourProductId"] = [ex.Message]
                });
            }
        });

        return endpoints;
    }
}
