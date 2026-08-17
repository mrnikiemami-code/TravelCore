using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.TripPlanner.Contracts;

namespace TravelCore.Modules.TripPlanner.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public Trip Planner HTTP surface (TC-P18-T008 / P18-R8).
/// Draft token secures anonymous TripIntent access — not identity authentication.
/// </summary>
internal static class TripPlannerPublicEndpoints
{
    public static IEndpointRouteBuilder MapTripPlannerPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(TripPlannerPublicCompositionBoundary.PublicApiGroup)
            .WithTags("TripPlannerPublic")
            .AllowAnonymous();

        group.MapPost("/intents", async Task<IResult> (
            TripPlannerCreateIntentRequest? request,
            ITripPlannerPublicCommand command,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await command.CreateIntentAsync(
                    request ?? new TripPlannerCreateIntentRequest(null),
                    cancellationToken);
                return Results.Created($"{TripPlannerPublicCompositionBoundary.PublicApiGroup}/intents/{created.IntentId}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapGet("/intents/{intentId:guid}", async Task<IResult> (
            Guid intentId,
            HttpContext httpContext,
            ITripPlannerPublicCommand command,
            CancellationToken cancellationToken) =>
        {
            if (!TryReadDraftToken(httpContext, out var draftToken))
            {
                return MissingDraftToken();
            }

            try
            {
                var draft = await command.GetIntentAsync(intentId, draftToken!, cancellationToken);
                return draft is null ? Results.NotFound() : Results.Ok(draft);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapPatch("/intents/{intentId:guid}", async Task<IResult> (
            Guid intentId,
            TripPlannerUpdateIntentRequest request,
            HttpContext httpContext,
            ITripPlannerPublicCommand command,
            CancellationToken cancellationToken) =>
        {
            if (!TryReadDraftToken(httpContext, out var draftToken))
            {
                return MissingDraftToken();
            }

            try
            {
                var draft = await command.UpdateIntentAsync(intentId, draftToken!, request, cancellationToken);
                return draft is null ? Results.NotFound() : Results.Ok(draft);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        group.MapPost("/intents/{intentId:guid}/submit", async Task<IResult> (
            Guid intentId,
            TripPlannerSubmitLeadRequest request,
            HttpContext httpContext,
            ITripPlannerPublicCommand command,
            CancellationToken cancellationToken) =>
        {
            if (!TryReadDraftToken(httpContext, out var draftToken))
            {
                return MissingDraftToken();
            }

            try
            {
                var submitted = await command.SubmitLeadAsync(intentId, draftToken!, request, cancellationToken);
                return submitted is null ? Results.NotFound() : Results.Ok(submitted);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Lead submission rejected.");
            }
        });

        return endpoints;
    }

    private static bool TryReadDraftToken(HttpContext httpContext, out string? draftToken)
    {
        if (httpContext.Request.Headers.TryGetValue(
                TripPlannerPublicCompositionBoundary.DraftTokenHeader,
                out var values)
            && !string.IsNullOrWhiteSpace(values.ToString()))
        {
            draftToken = values.ToString();
            return true;
        }

        draftToken = null;
        return false;
    }

    private static IResult MissingDraftToken() =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [TripPlannerPublicCompositionBoundary.DraftTokenHeader] =
                [$"{TripPlannerPublicCompositionBoundary.DraftTokenHeader} header is required."],
        });

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message],
        });
}
