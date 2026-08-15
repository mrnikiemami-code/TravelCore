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
        });

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
        });

        group.MapGet("/{id:guid}/translations", async Task<IResult> (
            Guid id,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var translations = await service.ListTranslationsAsync(id, cancellationToken);
            return Results.Ok(translations);
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
        });

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
