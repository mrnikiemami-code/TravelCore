using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Content.Contracts;

namespace TravelCore.Modules.Content.Infrastructure.Endpoints;

/// <summary>
/// Admin Content HTTP surface (TC-P08-T007/T008). Mutations require Access.Content.Items.Write.
/// No Delete/Archive (P08-R8). Content owns current translation Slug (P08-R3); SEO owns IndexPolicy (P08-R4).
/// No widgets (P08-R6).
/// </summary>
internal static class ContentEndpoints
{
    private const string ContentItemsWritePolicy = "Access.Content.Items.Write";

    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var items = endpoints.MapGroup("/api/content/items")
            .WithTags("Content");

        // Public-facing slug lookup defaults to title+slug gate (no CatalogStatus invent).
        items.MapGet("/by-slug/{localeCode}/{slug}", async Task<IResult> (
            string localeCode,
            string slug,
            bool? publicOnly,
            IContentItemService service,
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

        items.MapPost("/", async Task<IResult> (
            CreateContentItemRequest request,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/content/items/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapGet("/", async Task<IResult> (
            string? kind,
            int? take,
            IContentItemService service,
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

        items.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            string? locale,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            var item = await service.GetByIdAsync(id, locale, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        items.MapGet("/by-code/{code}", async Task<IResult> (
            string code,
            string? locale,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.GetByCodeAsync(code, locale, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        items.MapPut("/{id:guid}/translations/{localeCode}", async Task<IResult> (
            Guid id,
            string localeCode,
            UpsertContentItemTranslationRequest request,
            IContentItemService service,
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
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapPut("/{id:guid}/translations/{localeCode}/slug", async Task<IResult> (
            Guid id,
            string localeCode,
            SetContentItemTranslationSlugRequest request,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var translation = await service.SetTranslationSlugAsync(id, localeCode, request, cancellationToken);
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
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapGet("/{id:guid}/translations", async Task<IResult> (
            Guid id,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            var list = await service.ListTranslationsAsync(id, cancellationToken);
            return Results.Ok(list);
        });

        items.MapPost("/{id:guid}/categories/{categoryId:guid}", async Task<IResult> (
            Guid id,
            Guid categoryId,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.AssignCategoryAsync(id, categoryId, cancellationToken);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapDelete("/{id:guid}/categories/{categoryId:guid}", async Task<IResult> (
            Guid id,
            Guid categoryId,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.RemoveCategoryAsync(id, categoryId, cancellationToken);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapPost("/{id:guid}/tags/{tagId:guid}", async Task<IResult> (
            Guid id,
            Guid tagId,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.AssignTagAsync(id, tagId, cancellationToken);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapDelete("/{id:guid}/tags/{tagId:guid}", async Task<IResult> (
            Guid id,
            Guid tagId,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.RemoveTagAsync(id, tagId, cancellationToken);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapPost("/{id:guid}/destinations/{destinationId:guid}", async Task<IResult> (
            Guid id,
            Guid destinationId,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.AssignDestinationAsync(id, destinationId, cancellationToken);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapDelete("/{id:guid}/destinations/{destinationId:guid}", async Task<IResult> (
            Guid id,
            Guid destinationId,
            IContentItemService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.RemoveDestinationAsync(id, destinationId, cancellationToken);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        MapBlockEndpoints(items);
        MapTaxonomyEndpoints(endpoints);
        return endpoints;
    }

    private static void MapBlockEndpoints(RouteGroupBuilder items)
    {
        items.MapGet("/{id:guid}/blocks", async Task<IResult> (
            Guid id,
            IContentBlockService blocks,
            CancellationToken cancellationToken) =>
        {
            var list = await blocks.ListAsync(id, cancellationToken);
            return Results.Ok(list);
        });

        items.MapPost("/{id:guid}/blocks/heading", async Task<IResult> (
            Guid id,
            AddContentHeadingBlockRequest request,
            IContentBlockService blocks,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await blocks.AddHeadingAsync(id, request, cancellationToken));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapPost("/{id:guid}/blocks/paragraph", async Task<IResult> (
            Guid id,
            AddContentParagraphBlockRequest request,
            IContentBlockService blocks,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await blocks.AddParagraphAsync(id, request, cancellationToken));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapPost("/{id:guid}/blocks/image", async Task<IResult> (
            Guid id,
            AddContentImageBlockRequest request,
            IContentBlockService blocks,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await blocks.AddImageAsync(id, request, cancellationToken));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapPut("/{id:guid}/blocks/reorder", async Task<IResult> (
            Guid id,
            ReorderContentBlocksRequest request,
            IContentBlockService blocks,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await blocks.ReorderAsync(id, request, cancellationToken));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        items.MapDelete("/{id:guid}/blocks/{blockId:guid}", async Task<IResult> (
            Guid id,
            Guid blockId,
            IContentBlockService blocks,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await blocks.RemoveAsync(id, blockId, cancellationToken);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization(ContentItemsWritePolicy);
    }

    private static void MapTaxonomyEndpoints(IEndpointRouteBuilder endpoints)
    {
        var categories = endpoints.MapGroup("/api/content/categories").WithTags("Content");
        categories.MapGet("/", async Task<IResult> (
            int? take,
            IContentTaxonomyService taxonomy,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await taxonomy.ListCategoriesAsync(take ?? 100, cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        });

        categories.MapPost("/", async Task<IResult> (
            CreateContentCategoryRequest request,
            IContentTaxonomyService taxonomy,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await taxonomy.CreateCategoryAsync(request, cancellationToken);
                return Results.Created($"/api/content/categories/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(ContentItemsWritePolicy);

        var tags = endpoints.MapGroup("/api/content/tags").WithTags("Content");
        tags.MapGet("/", async Task<IResult> (
            int? take,
            IContentTaxonomyService taxonomy,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await taxonomy.ListTagsAsync(take ?? 100, cancellationToken));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        });

        tags.MapPost("/", async Task<IResult> (
            CreateContentTagRequest request,
            IContentTaxonomyService taxonomy,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await taxonomy.CreateTagAsync(request, cancellationToken);
                return Results.Created($"/api/content/tags/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(ContentItemsWritePolicy);
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
