using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Tour.Contracts;

namespace TravelCore.Modules.Tour.Infrastructure.Endpoints;

/// <summary>
/// Admin TourDeparture HTTP surface (TC-P11-T008). Execution management only.
/// </summary>
internal static class TourDepartureEndpoints
{
    private const string DeparturesReadPolicy = "Access.Tour.Departures.Read";
    private const string DeparturesWritePolicy = "Access.Tour.Departures.Write";

    public static IEndpointRouteBuilder MapTourDepartureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tour/departures")
            .WithTags("TourDepartures");

        group.MapPost("/", async Task<IResult> (
            CreateTourDepartureRequest request,
            ITourDepartureAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/tour/departures/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(DeparturesWritePolicy);

        group.MapGet("/", async Task<IResult> (
            Guid? tourProductId,
            string? status,
            int? take,
            ITourDepartureAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var list = await service.ListAsync(tourProductId, status, take ?? 50, cancellationToken);
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
        }).RequireAuthorization(DeparturesReadPolicy);

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            ITourDepartureAdminService service,
            CancellationToken cancellationToken) =>
        {
            var item = await service.GetAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).RequireAuthorization(DeparturesReadPolicy);

        group.MapPut("/{id:guid}/schedule", async Task<IResult> (
            Guid id,
            SetTourDepartureScheduleRequest request,
            ITourDepartureAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetScheduleAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(DeparturesWritePolicy);

        group.MapPut("/{id:guid}/capacity", async Task<IResult> (
            Guid id,
            SetTourDepartureCapacityRequest request,
            ITourDepartureAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetCapacityAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(DeparturesWritePolicy);

        group.MapPut("/{id:guid}/status", async Task<IResult> (
            Guid id,
            SetTourDepartureStatusRequest request,
            ITourDepartureAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.SetStatusAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(DeparturesWritePolicy);

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
