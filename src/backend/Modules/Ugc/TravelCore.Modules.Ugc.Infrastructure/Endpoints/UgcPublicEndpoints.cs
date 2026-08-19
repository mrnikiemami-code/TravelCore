using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Ugc.Contracts;

namespace TravelCore.Modules.Ugc.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public UGC composition reads (TC-P16-T008 / P16-R8).
/// Eligible facts only. Not SEO indexing authority and not a Search engine.
/// </summary>
internal static class UgcPublicEndpoints
{
    public static IEndpointRouteBuilder MapUgcPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicUgc = endpoints.MapGroup("/api/ugc/public")
            .WithTags("Ugc");

        publicUgc.MapGet("/reviews", async Task<IResult> (
            string targetType,
            Guid targetId,
            IUgcPublicReviewQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var page = await query.GetByTargetAsync(targetType, targetId, cancellationToken);
                return Results.Ok(page);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        publicUgc.MapGet("/travelogues", async Task<IResult> (
            string localeCode,
            IUgcPublicTravelogueQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.ListByLocaleAsync(localeCode, cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        publicUgc.MapGet("/travelogues/{travelogueId:guid}", async Task<IResult> (
            Guid travelogueId,
            IUgcPublicTravelogueQuery query,
            CancellationToken cancellationToken) =>
        {
            var item = await query.GetByIdAsync(travelogueId, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        publicUgc.MapGet("/user-photos", async Task<IResult> (
            IUgcPublicUserPhotoQuery query,
            CancellationToken cancellationToken) =>
        {
            var items = await query.ListAsync(cancellationToken);
            return Results.Ok(items);
        });

        publicUgc.MapGet("/comments", async Task<IResult> (
            string targetType,
            Guid targetId,
            IUgcPublicCommentQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.GetByTargetAsync(targetType, targetId, cancellationToken);
                return Results.Ok(items);
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
            [ex.ParamName ?? "value"] = [ex.Message]
        });
}
