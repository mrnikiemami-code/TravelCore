using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Infrastructure.Services;

namespace TravelCore.Modules.Destination.Infrastructure.Endpoints;

internal static class DestinationEndpoints
{
    public static IEndpointRouteBuilder MapDestinationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/destination/destinations")
            .WithTags("Destination");

        group.MapPost("/", async Task<IResult> (
            CreateDestinationRequest request,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/destination/destinations/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization("Access.Destination.Destinations.Write");

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            string? locale,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var destination = await service.GetByIdAsync(id, locale, cancellationToken);
            return destination is null ? Results.NotFound() : Results.Ok(destination);
        });

        group.MapGet("/{id:guid}/children", async Task<IResult> (
            Guid id,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var children = await service.ListChildrenAsync(id, cancellationToken);
            return Results.Ok(children);
        });

        group.MapPut("/{id:guid}/translations/{localeCode}", async Task<IResult> (
            Guid id,
            string localeCode,
            UpsertDestinationTranslationRequest request,
            DestinationApplicationService service,
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
        }).RequireAuthorization("Access.Destination.Destinations.Write");

        group.MapGet("/{id:guid}/translations", async Task<IResult> (
            Guid id,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var translations = await service.ListTranslationsAsync(id, cancellationToken);
            return Results.Ok(translations);
        });

        group.MapPut("/{id:guid}/translations/{localeCode}/slug", async Task<IResult> (
            Guid id,
            string localeCode,
            SetDestinationTranslationSlugRequest request,
            DestinationApplicationService service,
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
        }).RequireAuthorization("Access.Destination.Destinations.Write");

        group.MapGet("/by-slug/{localeCode}/{slug}", async Task<IResult> (
            string localeCode,
            string slug,
            IDestinationReadQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var hit = await query.FindBySlugAsync(localeCode, slug, cancellationToken);
                return hit is null ? Results.NotFound() : Results.Ok(hit);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapPut("/{id:guid}/geo", async Task<IResult> (
            Guid id,
            SetDestinationGeoRequest request,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetGeoAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        }).RequireAuthorization("Access.Destination.Destinations.Write");

        group.MapPut("/{id:guid}/media/cover", async Task<IResult> (
            Guid id,
            SetDestinationCoverRequest request,
            IDestinationMediaService service,
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
        }).RequireAuthorization("Access.Destination.Destinations.Write");

        group.MapDelete("/{id:guid}/media/cover", async Task<IResult> (
            Guid id,
            IDestinationMediaService service,
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
        }).RequireAuthorization("Access.Destination.Destinations.Write");

        group.MapGet("/{id:guid}/media", async Task<IResult> (
            Guid id,
            IDestinationMediaService service,
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
            IDestinationMediaService service,
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

        group.MapGet("/{id:guid}/ancestors", async Task<IResult> (
            Guid id,
            IDestinationReadQuery query,
            CancellationToken cancellationToken) =>
        {
            var path = await query.GetPathAsync(id, cancellationToken);
            return path is null ? Results.NotFound() : Results.Ok(path.AncestorsRootFirst);
        });

        group.MapGet("/{id:guid}/path", async Task<IResult> (
            Guid id,
            IDestinationReadQuery query,
            CancellationToken cancellationToken) =>
        {
            var path = await query.GetPathAsync(id, cancellationToken);
            return path is null ? Results.NotFound() : Results.Ok(path);
        });

        group.MapGet("/{id:guid}/descendants", async Task<IResult> (
            Guid id,
            int? depth,
            IDestinationReadQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var maxDepth = depth ?? 1;
                var descendants = await query.ListDescendantsAsync(id, maxDepth, cancellationToken);
                return descendants is null ? Results.NotFound() : Results.Ok(descendants);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "depth"] = [ex.Message]
                });
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
