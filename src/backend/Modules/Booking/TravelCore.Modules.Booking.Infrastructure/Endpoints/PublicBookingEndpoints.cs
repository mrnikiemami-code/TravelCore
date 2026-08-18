using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure.Services;

namespace TravelCore.Modules.Booking.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public Booking initiation and authorized private reads (TC-P19-T008 / P19-R8).
/// Pending initiation only — no confirmation, payment, listing, or public cancellation.
/// </summary>
internal static class PublicBookingEndpoints
{
    public static IEndpointRouteBuilder MapPublicBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(PublicBookingCompositionBoundary.PublicApiGroup)
            .WithTags("BookingPublic")
            .AllowAnonymous();

        group.MapPost("/initiations", async Task<IResult> (
            PublicBookingInitiationRequest? request,
            HttpContext httpContext,
            IPublicBookingInitiationService initiation,
            CancellationToken cancellationToken) =>
        {
            if (request is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["request"] = ["Request body is required."],
                });
            }

            var merged = MergeIdempotency(request, httpContext);
            try
            {
                var created = await initiation.InitiateAsync(
                    merged,
                    PublicBookingActorClaims.TryReadActorId(httpContext.User),
                    cancellationToken);
                return Results.Created(
                    $"{PublicBookingCompositionBoundary.PublicApiGroup}/{created.BookingId:D}",
                    created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InsufficientCapacityException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Insufficient TourDeparture capacity.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Public Booking initiation rejected.");
            }
        });

        group.MapGet("/{bookingId:guid}", async Task<IResult> (
            Guid bookingId,
            HttpContext httpContext,
            IPublicBookingReadService reads,
            CancellationToken cancellationToken) =>
        {
            httpContext.Request.Headers.TryGetValue(
                PublicBookingCompositionBoundary.AccessTokenHeader,
                out var tokenValues);
            var read = await reads.GetAuthorizedAsync(
                bookingId,
                tokenValues.ToString(),
                PublicBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            return read is null ? Results.NotFound() : Results.Ok(read);
        });

        return endpoints;
    }

    private static PublicBookingInitiationRequest MergeIdempotency(
        PublicBookingInitiationRequest request,
        HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return request;
        }

        if (httpContext.Request.Headers.TryGetValue(
                PublicBookingCompositionBoundary.IdempotencyHeader,
                out var values)
            && !string.IsNullOrWhiteSpace(values.ToString()))
        {
            return request with { IdempotencyKey = values.ToString() };
        }

        return request;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message],
        });
}
