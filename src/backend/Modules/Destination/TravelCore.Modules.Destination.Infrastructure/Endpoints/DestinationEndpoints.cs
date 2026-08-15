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
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            DestinationApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var destination = await service.GetByIdAsync(id, cancellationToken);
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

        return endpoints;
    }
}
