using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Ugc.Contracts;

namespace TravelCore.Modules.Ugc.Infrastructure.Endpoints;

/// <summary>
/// Admin UGC moderation HTTP surface (TC-MODOPS-T004).
/// Requires Access-backed UGC moderation policies. Not public composition reads.
/// </summary>
internal static class UgcAdminEndpoints
{
    private const string ModerationReadPolicy = "Access.Ugc.Moderation.Read";
    private const string ModerationModeratePolicy = "Access.Ugc.Moderation.Moderate";

    public static IEndpointRouteBuilder MapUgcAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ugc/moderation")
            .WithTags("Ugc");

        var travelogues = group.MapGroup("/travelogues");

        travelogues.MapGet("/pending", async Task<IResult> (
            int? take,
            IUgcModerationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await service.ListPendingTraveloguesAsync(take ?? 50, cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        }).RequireAuthorization(ModerationReadPolicy);

        travelogues.MapPost("/{travelogueId:guid}/approve", async Task<IResult> (
            Guid travelogueId,
            IUgcModerationService service,
            CancellationToken cancellationToken) =>
            await MutateTravelogue(travelogueId, service.ApproveTravelogueAsync, cancellationToken))
            .RequireAuthorization(ModerationModeratePolicy);

        travelogues.MapPost("/{travelogueId:guid}/reject", async Task<IResult> (
            Guid travelogueId,
            IUgcModerationService service,
            CancellationToken cancellationToken) =>
            await MutateTravelogue(travelogueId, service.RejectTravelogueAsync, cancellationToken))
            .RequireAuthorization(ModerationModeratePolicy);

        travelogues.MapPost("/{travelogueId:guid}/publish", async Task<IResult> (
            Guid travelogueId,
            IUgcModerationService service,
            CancellationToken cancellationToken) =>
            await MutateTravelogue(travelogueId, service.PublishTravelogueAsync, cancellationToken))
            .RequireAuthorization(ModerationModeratePolicy);

        return endpoints;
    }

    private static async Task<IResult> MutateTravelogue(
        Guid travelogueId,
        Func<Guid, CancellationToken, Task<ModerationQueueTravelogueItem>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await action(travelogueId, cancellationToken);
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
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Moderation lifecycle conflict");
        }
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "value"] = [ex.Message]
        });
}
