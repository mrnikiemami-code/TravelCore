using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public Booking initiation and authorized private reads/payment (TC-P19-T008 / TC-P20-T007).
/// Pending initiation and Booking-scoped Payment only — no confirmation, listing, or public cancellation.
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
            var read = await reads.GetAuthorizedAsync(
                bookingId,
                ReadAccessToken(httpContext),
                PublicBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            return read is null ? Results.NotFound() : Results.Ok(read);
        });

        group.MapGet("/{bookingId:guid}/payment", async Task<IResult> (
            Guid bookingId,
            HttpContext httpContext,
            IPublicBookingReadService reads,
            IPublicBookingPaymentService payments,
            CancellationToken cancellationToken) =>
        {
            var booking = await reads.GetAuthorizedAsync(
                bookingId,
                ReadAccessToken(httpContext),
                PublicBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            if (booking is null)
            {
                return Results.NotFound();
            }

            var payment = await payments.GetByBookingIdAsync(bookingId, cancellationToken);
            return Results.Ok(Compose(booking, payment));
        });

        group.MapPost("/{bookingId:guid}/payment/initiation", async Task<IResult> (
            Guid bookingId,
            PublicPaymentInitiationRequest? request,
            HttpContext httpContext,
            IPublicBookingReadService reads,
            IPublicBookingPaymentService payments,
            CancellationToken cancellationToken) =>
        {
            var booking = await reads.GetAuthorizedAsync(
                bookingId,
                ReadAccessToken(httpContext),
                PublicBookingActorClaims.TryReadActorId(httpContext.User),
                cancellationToken);
            if (booking is null)
            {
                return Results.NotFound();
            }

            var idempotency = request?.IdempotencyKey;
            if (string.IsNullOrWhiteSpace(idempotency)
                && httpContext.Request.Headers.TryGetValue(
                    PublicBookingCompositionBoundary.IdempotencyHeader,
                    out var values)
                && !string.IsNullOrWhiteSpace(values.ToString()))
            {
                idempotency = values.ToString();
            }

            var result = await payments.InitiateForBookingAsync(bookingId, idempotency, cancellationToken);
            var body = Compose(booking, result.Payment);
            return result.Status switch
            {
                PublicPaymentCommandStatus.ProviderUnavailable => Results.Problem(
                    detail: "Online payment is not currently available.",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Payment provider is not configured.",
                    extensions: new Dictionary<string, object?> { ["payment"] = body }),
                PublicPaymentCommandStatus.BookingIneligible => Results.Problem(
                    detail: "Booking is not eligible for Payment initiation.",
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Payment initiation rejected."),
                _ => Results.Ok(body),
            };
        });

        return endpoints;
    }

    private static string? ReadAccessToken(HttpContext httpContext)
    {
        httpContext.Request.Headers.TryGetValue(
            PublicBookingCompositionBoundary.AccessTokenHeader,
            out var tokenValues);
        return tokenValues.ToString();
    }

    private static PublicBookingPaymentRead Compose(PublicBookingRead booking, PublicPaymentRead payment) =>
        new(
            booking.BookingId,
            booking.Status,
            booking.Confirmed,
            payment.PaymentId,
            payment.PaymentStatus,
            payment.Amount ?? booking.Monetary?.TotalAmount,
            payment.CurrencyCode ?? booking.Monetary?.Currency,
            payment.ProviderInitiationPossible,
            payment.LatestAttemptStatus,
            payment.RefundStatus,
            payment.SafeAction,
            payment.RedirectUri);

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
