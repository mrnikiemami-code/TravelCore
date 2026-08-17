using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Tour.Contracts;

namespace TravelCore.Modules.Tour.Infrastructure.Endpoints;

/// <summary>
/// TourProduct HTTP surface (TC-P09-T008/T009/T010). Mutations require Access.Tour.Products.Write.
/// No Delete/Archive (P09-R4 closed as Draft|Published|Inactive). Tour owns current slug (P09-R5);
/// SEO owns IndexPolicy (P09-R6). Media presentation composes via Media.Contracts (app-proxy URLs).
/// </summary>
internal static class TourEndpoints
{
    private const string TourProductsWritePolicy = "Access.Tour.Products.Write";

    public static IEndpointRouteBuilder MapTourEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tour/products")
            .WithTags("Tour");

        group.MapPost("/", async Task<IResult> (
            CreateTourProductRequest request,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/tour/products/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapGet("/", async Task<IResult> (
            string? kind,
            int? take,
            ITourProductService service,
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

        group.MapGet("/by-code/{code}", async Task<IResult> (
            string code,
            string? locale,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var product = await service.GetByCodeAsync(code, locale, cancellationToken);
                return product is null ? Results.NotFound() : Results.Ok(product);
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

        group.MapPut("/{id:guid}/translations/{localeCode}", async Task<IResult> (
            Guid id,
            string localeCode,
            UpsertTourProductTranslationRequest request,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var translation = await service.UpsertTranslationAsync(id, localeCode, request, cancellationToken);
                return Results.Ok(translation);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/translations/{localeCode}/slug", async Task<IResult> (
            Guid id,
            string localeCode,
            SetTourProductTranslationSlugRequest request,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetTranslationSlugAsync(id, localeCode, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/catalog-status", async Task<IResult> (
            Guid id,
            SetTourCatalogStatusRequest request,
            ITourProductService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetCatalogStatusAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapGet("/{id:guid}/semantic-links", async Task<IResult> (
            Guid id,
            ITourProductSemanticLinkService service,
            CancellationToken cancellationToken) =>
        {
            var links = await service.GetAsync(id, cancellationToken);
            return links is null ? Results.NotFound() : Results.Ok(links);
        });

        group.MapPut("/{id:guid}/classification", async Task<IResult> (
            Guid id,
            SetTourClassificationRequest request,
            ITourProductSemanticLinkService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetClassificationAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/origin", async Task<IResult> (
            Guid id,
            SetTourOriginRequest request,
            ITourProductSemanticLinkService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetOriginAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/agency", async Task<IResult> (
            Guid id,
            SetTourAgencyRequest request,
            ITourProductSemanticLinkService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetAgencyAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPost("/{id:guid}/destinations/{destinationId:guid}", async Task<IResult> (
            Guid id,
            Guid destinationId,
            ITourProductSemanticLinkService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.AssignDestinationAsync(id, destinationId, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapDelete("/{id:guid}/destinations/{destinationId:guid}", async Task<IResult> (
            Guid id,
            Guid destinationId,
            ITourProductSemanticLinkService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.RemoveDestinationAsync(id, destinationId, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapGet("/{id:guid}/catalog-facts", async Task<IResult> (
            Guid id,
            ITourProductCatalogFactService service,
            CancellationToken cancellationToken) =>
        {
            var facts = await service.GetAsync(id, cancellationToken);
            return facts is null ? Results.NotFound() : Results.Ok(facts);
        });

        group.MapPut("/{id:guid}/services", async Task<IResult> (
            Guid id,
            ReplaceTourCatalogFactsRequest request,
            ITourProductCatalogFactService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReplaceServicesAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/policies", async Task<IResult> (
            Guid id,
            ReplaceTourCatalogFactsRequest request,
            ITourProductCatalogFactService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReplacePoliciesAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/requirements", async Task<IResult> (
            Guid id,
            ReplaceTourCatalogFactsRequest request,
            ITourProductCatalogFactService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReplaceRequirementsAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapGet("/{id:guid}/media", async Task<IResult> (
            Guid id,
            ITourProductMediaService service,
            CancellationToken cancellationToken) =>
        {
            var media = await service.GetAsync(id, cancellationToken);
            return media is null ? Results.NotFound() : Results.Ok(media);
        });

        // Public compose: Tour-owned Cover/Gallery + Media.Contracts presentation (app-proxy).
        group.MapGet("/{id:guid}/media/presentation", async Task<IResult> (
            Guid id,
            string? locale,
            ITourProductMediaService service,
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

        group.MapPut("/{id:guid}/media/cover", async Task<IResult> (
            Guid id,
            SetTourCoverRequest request,
            ITourProductMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetCoverAsync(id, request.MediaAssetId, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapDelete("/{id:guid}/media/cover", async Task<IResult> (
            Guid id,
            ITourProductMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.RemoveCoverAsync(id, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPost("/{id:guid}/media/gallery", async Task<IResult> (
            Guid id,
            AddTourGalleryItemRequest request,
            ITourProductMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.AddGalleryItemAsync(
                    id,
                    request.MediaAssetId,
                    request.SortOrder,
                    cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapDelete("/{id:guid}/media/gallery/{mediaAssetId:guid}", async Task<IResult> (
            Guid id,
            Guid mediaAssetId,
            ITourProductMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.RemoveGalleryItemAsync(id, mediaAssetId, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        group.MapPut("/{id:guid}/media/gallery/order", async Task<IResult> (
            Guid id,
            ReorderTourGalleryRequest request,
            ITourProductMediaService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReorderGalleryAsync(id, request.OrderedMediaAssetIds, cancellationToken);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(TourProductsWritePolicy);

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}

internal sealed record SetTourCoverRequest(Guid MediaAssetId);

internal sealed record AddTourGalleryItemRequest(Guid MediaAssetId, int? SortOrder = null);

internal sealed record ReorderTourGalleryRequest(IReadOnlyList<Guid> OrderedMediaAssetIds);
