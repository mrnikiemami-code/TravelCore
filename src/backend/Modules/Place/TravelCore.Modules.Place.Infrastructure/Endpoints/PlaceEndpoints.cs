using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Place.Contracts;

namespace TravelCore.Modules.Place.Infrastructure.Endpoints;

/// <summary>
/// Admin Place catalog HTTP surface (TC-P07-T006/T007). Mutations require Access.Place.Places.Write.
/// No Delete/Archive (P07-R3). Place owns current translation Slug (P07-R4); SEO owns indexability (P07-R5).
/// </summary>
internal static class PlaceEndpoints
{
    private const string PlacePlacesWritePolicy = "Access.Place.Places.Write";

    public static IEndpointRouteBuilder MapPlaceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/place/places")
            .WithTags("Place");

        group.MapPost("/", async Task<IResult> (
            CreatePlaceRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/place/places/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapGet("/", async Task<IResult> (
            string? kind,
            int? take,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var list = await service.ListAsync(kind, take ?? 50, cancellationToken);
                return Results.Ok(list);
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

        group.MapGet("/by-code/{code}", async Task<IResult> (
            string code,
            string? locale,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var place = await service.GetByCodeAsync(code, locale, cancellationToken);
                return place is null ? Results.NotFound() : Results.Ok(place);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        // Public-facing slug lookup defaults to Active-only (Draft/Inactive → 404).
        group.MapGet("/by-slug/{localeCode}/{slug}", async Task<IResult> (
            string localeCode,
            string slug,
            bool? publicOnly,
            IPlaceService service,
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
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            var place = await service.GetByIdAsync(id, locale, cancellationToken);
            return place is null ? Results.NotFound() : Results.Ok(place);
        });

        group.MapPut("/{id:guid}/translations/{localeCode}", async Task<IResult> (
            Guid id,
            string localeCode,
            UpsertPlaceTranslationRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var translation = await service.UpsertTranslationAsync(id, localeCode, request, cancellationToken);
                return Results.Ok(translation);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/translations/{localeCode}/slug", async Task<IResult> (
            Guid id,
            string localeCode,
            SetPlaceTranslationSlugRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var translation = await service.SetTranslationSlugAsync(id, localeCode, request, cancellationToken);
                return Results.Ok(translation);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapGet("/{id:guid}/translations", async Task<IResult> (
            Guid id,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var translations = await service.ListTranslationsAsync(id, cancellationToken);
                return Results.Ok(translations);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapPut("/{id:guid}/destination-link", async Task<IResult> (
            Guid id,
            SetPlaceDestinationLinkRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetDestinationLinkAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/geo", async Task<IResult> (
            Guid id,
            SetPlaceGeoRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetGeoAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/address", async Task<IResult> (
            Guid id,
            SetPlaceAddressRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetAddressAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/catalog-status", async Task<IResult> (
            Guid id,
            SetPlaceCatalogStatusRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetCatalogStatusAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/classification", async Task<IResult> (
            Guid id,
            SetPlaceClassificationRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetClassificationAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/facilities", async Task<IResult> (
            Guid id,
            SetPlaceFacilitiesRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetFacilitiesAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/media/cover", async Task<IResult> (
            Guid id,
            SetPlaceCoverRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var link = await service.SetCoverAsync(id, request, cancellationToken);
                return Results.Ok(link);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapDelete("/{id:guid}/media/cover", async Task<IResult> (
            Guid id,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await service.RemoveCoverAsync(id, cancellationToken);
                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPost("/{id:guid}/media/gallery", async Task<IResult> (
            Guid id,
            AddPlaceGalleryItemRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var link = await service.AddGalleryItemAsync(id, request, cancellationToken);
                return Results.Ok(link);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapDelete("/{id:guid}/media/gallery/{mediaAssetId:guid}", async Task<IResult> (
            Guid id,
            Guid mediaAssetId,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await service.RemoveGalleryItemAsync(id, mediaAssetId, cancellationToken);
                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapPut("/{id:guid}/media/gallery/order", async Task<IResult> (
            Guid id,
            ReorderPlaceGalleryRequest request,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var links = await service.ReorderGalleryAsync(id, request, cancellationToken);
                return Results.Ok(links);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PlacePlacesWritePolicy);

        group.MapGet("/{id:guid}/media", async Task<IResult> (
            Guid id,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var links = await service.ListMediaLinksAsync(id, cancellationToken);
                return Results.Ok(links);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapGet("/{id:guid}/media/presentation", async Task<IResult> (
            Guid id,
            string? locale,
            IPlaceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var presentation = await service.GetMediaPresentationAsync(id, locale, cancellationToken);
                return presentation is null ? Results.NotFound() : Results.Ok(presentation);
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
