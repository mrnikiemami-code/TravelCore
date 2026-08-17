using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Tour.Contracts;

namespace TravelCore.Modules.Tour.Infrastructure.Endpoints;

/// <summary>
/// Public TourProduct read surface for publishing + SEO route prep (TC-P09-T008).
/// Mutations stay service-level this task (Admin Tour job deferred when not in Auto-Execute Allowed).
/// </summary>
internal static class TourEndpoints
{
    public static IEndpointRouteBuilder MapTourEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tour/products")
            .WithTags("Tour");

        // Public-facing slug lookup defaults to Published-only (Draft/Inactive → 404).
        group.MapGet("/by-slug/{localeCode}/{slug}", async Task<IResult> (
            string localeCode,
            string slug,
            bool? publicOnly,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var hit = await service.FindBySlugAsync(
                    localeCode,
                    slug,
                    publicOnly ?? true,
                    cancellationToken);
                return hit is null ? Results.NotFound() : Results.Ok(hit);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            string? locale,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            var product = await service.GetAsync(id, locale, cancellationToken);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
